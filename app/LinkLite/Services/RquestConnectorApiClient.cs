using Flurl;
using LinkLite.Dto;
using LinkLite.OptionsModels;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http;
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
        /// Try and get a job for a biobank
        /// </summary>
        /// <param name="collectionId">RQUEST Collection Id (Biobank Id)</param>
        /// <returns></returns>
        public async Task<RquestQuery?> FetchQuery(string collectionId)
        {
            var result = await _client.GetAsync(
                _apiOptions.FetchQueryEndpoint
                .AppendPathSegment(collectionId));

            if (result.IsSuccessStatusCode)
            {
                try
                {
                    var query = await JsonSerializer.DeserializeAsync<RquestQuery>(
                        await result.Content.ReadAsStreamAsync());

                    if (string.IsNullOrWhiteSpace(query?.TaskId))
                    {
                        _logger.LogInformation(
                            "No Query Tasks waiting for {collectionId}",
                            collectionId);
                        return query;
                    }

                    if (query.Task is null)
                    {
                        var message = $"Found Task Id ({query.TaskId}) but no Task.";
                        _logger.LogError(message);
                        throw new ApplicationException(message);
                    }

                    _logger.LogInformation($"Found Task with Id: {query.TaskId}");
                    return query;
                }
                catch (JsonException e)
                {
                    _logger.LogError(e, "Invalid Response Format from Fetch Query Endpoint");
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
    }
}
