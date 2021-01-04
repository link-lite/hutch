namespace LinkLite.OptionsModels
{
    public class RquestPollingServiceOptions
    {
        /// <summary>
        /// Polling interval in seconds
        /// for fetching queries from the RQUEST Connector API
        /// </summary>
        public int QueryPollingInterval { get; set; } = 5;

        /// <summary>
        /// 
        /// </summary>
        public string RquestCollectionId { get; set; } = "";
    }
}
