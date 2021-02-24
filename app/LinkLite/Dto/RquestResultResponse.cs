using System.Text.Json.Serialization;

namespace LinkLite.Dto
{
    /// <summary>
    /// Response body for "successful" (200 OK) Results Submission requests
    /// </summary>
    public class RquestResultResponse
    {
        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;
    }
}
