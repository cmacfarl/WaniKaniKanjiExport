using System;
using System.Net;
using System.Text;
using Newtonsoft.Json;

public class JsonUtilities
{
    public static T downloadSerializedJsonData<T>(Uri url, string credentials) where T : new()
    {
        using (var w = new WebClient()) {
            w.Headers[HttpRequestHeader.Authorization] = $"Bearer {credentials}";
            w.Headers[HttpRequestHeader.ContentType] = "application/json";
            w.Encoding = Encoding.UTF8;
            var json_data = string.Empty;
            // attempt to download JSON data as a string
            try {
                json_data = w.DownloadString(url);
            } catch (Exception ex) {
                // throw new FMSException($"FMS error downloading json formatted data for {typeof(T).Name}", ex);
            }

            // if string with JSON data is not empty, deserialize it to class and return its instance 
            JsonSerializerSettings settings = new JsonSerializerSettings {
                NullValueHandling = NullValueHandling.Ignore
            };
            return !string.IsNullOrEmpty(json_data) ? JsonConvert.DeserializeObject<T>(json_data, settings) : new T();
        }
    }
}