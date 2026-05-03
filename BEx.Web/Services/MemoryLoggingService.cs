using System.Diagnostics;

namespace BEx.Web.Services
{
    public class MemoryLoggingService : BackgroundService
    {
        private readonly GameSessionManager _manager;

        public MemoryLoggingService(GameSessionManager manager)
        {
            _manager = manager;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                long managed = GC.GetTotalMemory(false) / 1024 / 1024;

                long working = Process.GetCurrentProcess().WorkingSet64 / 1024 / 1024;

                Console.WriteLine(
                    $"[MEMORY] Sessions={_manager.GetSessionCount()} | Managed={managed}MB | WorkingSet={working}MB"
                );

                await Task.Delay(
                    TimeSpan.FromMinutes(5),
                    stoppingToken
                );
            }
        }
    }
}