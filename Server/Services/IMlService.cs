using System.Text.Json.Serialization;
using Server.Models;

namespace Server.Services;

public interface IMlService
{
    Task<MlResult> PerformMlAsync(string url, string key);
}

public class MlService(HttpClient httpClient) : IMlService
{
    public async Task<MlResult> PerformMlAsync(string url, string key)
    {
        var httpResponse = await httpClient.PostAsJsonAsync("compute_photo", new MlRequest() { Url = url, Filename = key});
        httpResponse.EnsureSuccessStatusCode();
        var mlResponse = await httpResponse.Content.ReadFromJsonAsync<MlResponse>();
        var result = new MlResult()
        {
            IsDetected = mlResponse!.Annotations.Count != 0,
            Detections =
            [
                .. mlResponse!.Annotations
                    .Select(a => new Detection()
                    {
                        Class = a.Class,
                        H = a.H,
                        W = a.W,
                        Xc = a.Xc,
                        Yc = a.Yc,
                    })
            ],
        };
        return result;
    }

    private class MlRequest
    {
        [JsonPropertyName("download_url")] public required string Url { get; set; }
        [JsonPropertyName("filename")] public required string Filename { get; set; }
    }

    // Root myDeserializedClass = JsonSerializer.Deserialize<Root>(myJsonResponse);
    public class Annotation
    {
        [JsonPropertyName("xc")]
        public double Xc { get; set; }

        [JsonPropertyName("yc")]
        public double Yc { get; set; }

        [JsonPropertyName("w")]
        public double W { get; set; }

        [JsonPropertyName("h")]
        public double H { get; set; }

        [JsonPropertyName("class")]
        public int Class { get; set; }

        [JsonPropertyName("file_name")]
        public string FileName { get; set; }
    }

    public class MlResponse
    {
        [JsonPropertyName("annotations")]
        public List<Annotation> Annotations { get; set; }

        [JsonPropertyName("images")]
        public List<string> Images { get; set; }
    }
}