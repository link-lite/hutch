namespace LinkLite.OptionsModels
{
    /// <summary>
    /// Options for the RQUEST Connector Api
    /// </summary>
    public class RquestConnectorApiOptions
    {
        /// <summary>
        /// The Base Url of the API
        /// </summary>
        public string BaseUrl { get; set; } = string.Empty;

        /// <summary>
        /// Fetch Query Endpoint
        /// </summary>
        public string FetchQueryEndpoint { get; set; } = "fetch_query";

        /// <summary>
        /// Submit Result Endpoint
        /// </summary>
        public string SubmitResultEndpoint { get; set; } = "submit_result";
    }
}
