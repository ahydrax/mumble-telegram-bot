using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace KNFA.Bots.MTB.Services
{
    public abstract class BackgroundService : IHostedService
    {
        private CancellationTokenSource? _workerCancellationTokenSource;
        private Task? _workerTask;

        public async Task StartAsync(CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            await OnStart(ct);

            _workerCancellationTokenSource = new CancellationTokenSource();
            _workerTask = Task.Factory.StartNew(
                    async () => await InternalLoop(_workerCancellationTokenSource.Token),
                    _workerCancellationTokenSource.Token,
                    TaskCreationOptions.LongRunning,
                    TaskScheduler.Default)
                .Unwrap();
        }

        protected abstract ILogger Logger { get; }

        protected abstract Task OnStart(CancellationToken ct);

        protected abstract Task Execute(CancellationToken ct);

        protected abstract Task OnStop(CancellationToken ct);

        protected abstract void OnError(Exception e);

        private async Task InternalLoop(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    await Execute(ct);
                }
                catch (Exception e)
                {
                    Logger.LogError(e, "Worker failed! Exiting execution loop...");
                    OnError(e);
                }
            }
        }

        public async Task StopAsync(CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            await OnStop(ct);

            if (_workerTask == null) return;

            try
            {
                _workerCancellationTokenSource?.Cancel();
            }
            finally
            {
                await Task.WhenAny(_workerTask, Task.Delay(TimeSpan.FromSeconds(5), ct));
            }
        }
    }
}
