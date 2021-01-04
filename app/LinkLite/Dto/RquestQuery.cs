using System.Text.Json.Serialization;

namespace LinkLite.Dto
{
    public class RquestQuery
    {
        [JsonPropertyName("task_id")]
        public string TaskId { get; set; } = string.Empty;

        public RquestQueryTask? Task {get;set;}
    }
}
