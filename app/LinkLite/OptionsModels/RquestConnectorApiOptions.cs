namespace LinkLite.OptionsModels
{
    /// <summary>
    /// Options for the RQUEST Connector Api
    /// </summary>
    public class RquestConnectorApiOptions
    {
        /// <summary>
        /// The Base Url of the Connector API, not just RQUEST.
        /// 
        /// Expected to be something like `[rquest-domain]/task/capi/`
        /// </summary>
        public string BaseUrl { get; set; } = string.Empty;

        /// <summary>
        /// Queue Status Endpoint
        /// </summary>
        public string QueueStatusEndpoint { get; set; } = "queue";

        /// <summary>
        /// Fetch Query Endpoint
        /// </summary>
        public string FetchQueryEndpoint { get; set; } = "query";

        /// <summary>
        /// Submit Result Endpoint
        /// </summary>
        public string SubmitResultEndpoint { get; set; } = "result";
    }
}
