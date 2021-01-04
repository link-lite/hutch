using LinkLite.OptionsModels;
using LinkLite.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LinkLite.HostedServices
{
    public class RquestPollingService : IHostedService, IDisposable
    {
        private readonly ILogger<RquestPollingService> _logger;
        private readonly RquestConnectorApiClient _rquestApi;
        private readonly RquestPollingServiceOptions _config;
        private Timer? _timer;

        public RquestPollingService(
            ILogger<RquestPollingService> logger,
            RquestConnectorApiClient rquestApi,
            IOptions<RquestPollingServiceOptions> config)
        {
            _logger = logger;
            _rquestApi = rquestApi;
            _config = config.Value;
        }


        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("RQUEST Polling Service started.");
            _timer = new Timer(
                PollRquest,
                null,
                TimeSpan.Zero,
                TimeSpan.FromSeconds(_config.QueryPollingInterval));
            return Task.CompletedTask;
        }

        private async void PollRquest(object? state)
        {
            // async void here is intentional to meet the TimerCallback signature
            // Stephen Cleary says it's ok:
            // https://stackoverflow.com/a/38918443
            _logger.LogInformation(
                "Polling RQUEST for Queries on Collection: {_collectionId}",
                _config.RquestCollectionId);

            var query = await _rquestApi.FetchQuery(_config.RquestCollectionId);

            if (query?.Task is null)
            {
                _logger.LogInformation(
                      "No Queries on Collection: {_collectionId}",
                      _config.RquestCollectionId);
                return;
            }

            // TODO: Threading / Parallel query handling?
            // TODO: pause polling while processing (allow up to max parallellism)?

            // TODO: Process query - Query OMOP
            // TODO: submit results via Rquest API
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("RQUEST Polling Service stopping.");
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
