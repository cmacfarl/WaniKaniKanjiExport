
using System.Net.Mime;
using System.Text;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddAWSLambdaHosting(LambdaEventSource.RestApi);
var app = builder.Build();

List<string> GetKanjiItems(string stages, string authToken)
{
    Uri url;
    AssignmentCollection data;
    string baseUrl = "https://api.wanikani.com/v2/";
    string assignmentUrl = baseUrl + "assignments?subject_types=kanji&srs_stages=" + stages;
    List<string> kanji = new List<string>();
    List<int> subjects = new List<int>();
    
    do {
        url = new Uri(assignmentUrl);
        data = JsonUtilities.downloadSerializedJsonData<AssignmentCollection>(url, authToken);
        foreach (Assignment assignment in data.data) {
            subjects.Add(assignment.data.subject_id);
        }
        
        url = new Uri(new Uri(baseUrl), "subjects?ids=" + String.Join(",", subjects));
        ItemCollection items = JsonUtilities.downloadSerializedJsonData<ItemCollection>(url, authToken);
        foreach (Item item in items.data) {
            kanji.Add(item.data.slug);
        }
        
        subjects.Clear();
        assignmentUrl = data.pages.next_url;
    } while (assignmentUrl != null);
    
    return kanji;
}

app.MapGet(
    "/export/json", (string? stages, string auth_token) => 
    {
        string defaultStages = "0,1,2,3,4,5,6,7,8,9";
            
        if (stages == null) {
            stages = defaultStages;
        }
        
        List<string> kanji = GetKanjiItems(stages, auth_token);
        
        return kanji;
    });
    
app.MapGet(
    "/export/text", (string? stages, string auth_token) => 
    {
        string defaultStages = "0,1,2,3,4,5,6,7,8,9";
            
        if (stages == null) {
            stages = defaultStages;
        }
        
        List<string> kanji = GetKanjiItems(stages, auth_token);
        
        return new PlainTextResult(String.Join("", kanji));
    });
    
app.Run();

class PlainTextResult : IResult
{
    private readonly string _text;

    public PlainTextResult(string text)
    {
        _text = text;
    }

    public Task ExecuteAsync(HttpContext httpContext)
    {
        httpContext.Response.StatusCode = 200;
        httpContext.Response.ContentType = "text/plain; charset=utf-8";
        httpContext.Response.ContentLength = Encoding.UTF8.GetByteCount(_text);
        return httpContext.Response.WriteAsync(_text);
    }
}
