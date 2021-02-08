using LinkLite.Data;
using LinkLite.Dto;
using LinkLite.OptionsModels;
using LinkLite.Services;
using LinkLite.Services.QueryServices;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using System;
using System.Linq;
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
        private readonly RquestOmopQueryService _queries;

        public RquestPollingService(
            ILogger<RquestPollingService> logger,
            RquestConnectorApiClient rquestApi,
            RquestOmopQueryService queries,
            IOptions<RquestPollingServiceOptions> config)
        {
            _logger = logger;
            _rquestApi = rquestApi;
            _config = config.Value;
            _queries = queries;
        }


        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("RQUEST Polling Service started.");
            _timer = new Timer(PollRquest);
            RunTimerOnce();
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


            RquestQueryTask? task = null;
            int? result = null;

            try
            {
                task = await _rquestApi.FetchQuery(_config.RquestCollectionId);

                if (task is null)
                {
                    _logger.LogInformation(
                          "No Queries on Collection: {_collectionId}",
                          _config.RquestCollectionId);
                    RunTimerOnce();
                    return;
                }

                // TODO: Threading / Parallel query handling?
                // affects timer usage, the process logic will need to be
                // threaded using Task.Run or similar.
                //StopTimer();

                _logger.LogInformation("Processing Query: {taskId}", task.TaskId);

                result = await _queries.Process(task.Query);

                _logger.LogInformation(
                    "Query Result: {taskId}: {result}", task.TaskId, result);

                await _rquestApi.SubmitQueryResult(task.TaskId, result.Value);
            }
            catch (Exception e)
            {
                if (task is null)
                {
                    _logger.LogError(e, "Task fetching failed");
                }
                else
                {

                    if (result is null)
                    {
                        _logger.LogError(e,
                            "Query execution failed for task: {taskId}",
                            task.TaskId);
                    }
                    else
                    {
                        _logger.LogError(e,
                            "Results Submission failed for task: {taskId}, result: {result}",
                            task.TaskId,
                            result);
                    }

                    await _rquestApi.CancelQueryTask(task.TaskId);
                    _logger.LogInformation("Cancelled failed task: {taskId}", task.TaskId);
                }
            }

            RunTimerOnce();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("RQUEST Polling Service stopping.");
            StopTimer();
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }

        /// <summary>
        /// Run the timer one time, to execute its callback when it expires
        /// (after the configured polling interval time)
        /// </summary>
        private void RunTimerOnce()
            => _timer?.Change(
                TimeSpan.FromSeconds(_config.QueryPollingInterval),
                Timeout.InfiniteTimeSpan);

        /// <summary>
        /// Change the timer to execute its callback regularly
        /// at the configured polling interval
        /// </summary>
        private void StartTimer()
            => _timer?.Change(
                TimeSpan.Zero,
                TimeSpan.FromSeconds(_config.QueryPollingInterval));

        /// <summary>
        /// Change the timer to stop regular callback execution
        /// </summary>
        private void StopTimer()
            => _timer?.Change(Timeout.Infinite, 0);
    }
}
