using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LinkLite.HostedServices
{
    public class RquestPollingService : IHostedService, IDisposable
    {
        private readonly ILogger<RquestPollingService> _logger;
        private Timer _timer;

        public RquestPollingService(ILogger<RquestPollingService> logger)
        {
            _logger = logger;
        }


        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("RQUEST Polling Service started.");
            // TODO: Make polling interval configurable
            _timer = new Timer(PollRquest, null, TimeSpan.Zero, TimeSpan.FromSeconds(5));
            return Task.CompletedTask;
        }

        private async void PollRquest(object state)
        {
            // async void here is intentional to meet the TimerCallback signature
            // Stephen Cleary says it's ok:
            // https://stackoverflow.com/a/38918443

            _logger.LogInformation("Pretend like we just made an HTTP Request.");

            // TODO: await RQUEST API methods
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
