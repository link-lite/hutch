using System.Text.Json.Serialization;

namespace LinkLite.Dto
{
    /// <summary>
    /// Task payload returned from RQUEST Connector API /query endpoint
    /// </summary>
    public class RquestQueryTask
    {
        [JsonPropertyName("task_id")]
        public string TaskId { get; set; } = string.Empty;

        [JsonPropertyName("cohort")]
        public RquestQuery Query { get; set; } = new();
    }
}
