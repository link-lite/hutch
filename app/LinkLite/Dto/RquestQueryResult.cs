using System.Text.Json.Serialization;

namespace LinkLite.Dto
{
    /// <summary>
    /// Results payload for a Query Task.
    /// Results with no count can be used for task cancellation
    /// </summary>
    public class RquestQueryTaskResult
    {
        public RquestQueryTaskResult(string taskId, int? count = null)
        {
            TaskId = taskId;

            if(count.HasValue)
                QueryResult = new() { Count = count.Value };
        }

        [JsonPropertyName("task_id")]
        public string TaskId { get; set; }

        [JsonPropertyName("query_result")]
        public RquestQueryResult? QueryResult { get; set; }
    }

    public class RquestQueryResult
    {
        [JsonPropertyName("count")]
        public int Count { get; set; }
    }
}
