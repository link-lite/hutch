using Flurl;

using LinkLite.Dto;
using LinkLite.OptionsModels;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace LinkLite.Services
{
    public class RquestConnectorApiClient
    {
        private readonly HttpClient _client;
        private readonly ILogger<RquestConnectorApiClient> _logger;
        private readonly RquestConnectorApiOptions _apiOptions;

        public RquestConnectorApiClient(
            HttpClient client,
            ILogger<RquestConnectorApiClient> logger,
            IOptions<RquestConnectorApiOptions> apiOptions)
        {
            _client = client;
            _logger = logger;
            _apiOptions = apiOptions.Value;

            _client.BaseAddress = new Uri(Url.Combine(_apiOptions.BaseUrl, "/"));
        }

        /// <summary>
        /// Serialize a value to a JSON string, and provide HTTP StringContent
        /// for it with a media type of "application/json"
        /// </summary>
        /// <param name="value"></param>
        /// <returns>HTTP StringContent with the value serialized to JSON and a media type of "application/json"</returns>
        private StringContent AsHttpJsonString<T>(T value)
            => new StringContent(
                    JsonSerializer.Serialize(value),
                    System.Text.Encoding.UTF8,
                    "application/json");

        /// <summary>
        /// Try and get a job for a biobank
        /// </summary>
        /// <param name="collectionId">RQUEST Collection Id (Biobank Id)</param>
        /// <returns>A Task DTO containing a Query to run, or null if none are waiting</returns>
        public async Task<RquestQueryTask?> FetchQuery(string collectionId)
        {
            var result = await _client.PostAsync(
                _apiOptions.FetchQueryEndpoint,
                AsHttpJsonString(new { collection_id = collectionId }));

            if (result.IsSuccessStatusCode)
            {
                if (result.StatusCode == HttpStatusCode.NoContent)
                {
                    _logger.LogInformation(
                        "No Query Tasks waiting for {collectionId}",
                        collectionId);
                    return null;
                }

                try
                {
                    var task = await result.Content.ReadFromJsonAsync<RquestQueryTask>();

                    // a null task is impossible because the necessary JSON payload
                    // to achieve it would fail deserialization
                    _logger.LogInformation($"Found Query Task with Id: {task!.TaskId}");
                    return task;
                }
                catch (JsonException e)
                {
                    _logger.LogError(e, "Invalid Response Format from Fetch Query Endpoint");

                    var body = await result.Content.ReadAsStringAsync();
                    _logger.LogDebug("Invalid Response Body: {body}", body);

                    throw;
                }
            }
            else
            {
                var message = $"Fetch Query Endpoint Request failed: {result.StatusCode}";
                _logger.LogError(message);
                throw new ApplicationException(message);
            }
        }

        /// <summary>
        /// Submit the result of a query
        /// </summary>
        /// <param name="taskId">ID of the query task</param>
        /// <param name="count">The result</param>
        public async Task SubmitQueryResult(string taskId, int count) => await ResultsEndpointPost(taskId, count);

        /// <summary>
        /// Cancel a query task
        /// </summary>
        /// <param name="taskId">ID of the query task</param>
        public async Task CancelQueryTask(string taskId) => await ResultsEndpointPost(taskId);

        /// <summary>
        /// Post to the Results endpoint, and handle the response correctly
        /// </summary>
        /// <param name="taskId">Task ID</param>
        /// <param name="count">Optional Count for submitting results</param>
        private async Task ResultsEndpointPost(string taskId, int? count = null)
        {
            var response = (await _client.PostAsync(
                    _apiOptions.SubmitResultEndpoint,
                    AsHttpJsonString(new RquestQueryTaskResult(taskId, count))))
                .EnsureSuccessStatusCode();

            // however, even if 2xx we need to check the body for sucess status
            string body = string.Empty;
            try
            {
                body = await response.Content.ReadAsStringAsync();
                var json = JsonSerializer.Deserialize<RquestResultResponse>(body);

                if (json?.Status != "OK")
                {
                    var message = "Unsuccessful Response from Submit Results Endpoint";
                    _logger.LogError(message);
                    _logger.LogDebug("Response Body: {body}", body);

                    throw new ApplicationException(message);
                }

                return;
            }
            catch (JsonException e)
            {
                _logger.LogError(e, "Invalid Response Format from Submit Results Endpoint");
                _logger.LogDebug("Invalid Response Body: {body}", body);

                throw;
            }
        }
    }
}
