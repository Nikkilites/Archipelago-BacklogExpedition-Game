namespace BEx.Web.Services
{
    public class SessionCleanupService : BackgroundService
    {
        private readonly GameSessionManager _manager;

        public SessionCleanupService(GameSessionManager manager)
        {
            _manager = manager;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await _manager.CleanupExpiredSessions();
                await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken);
            }
        }
    }
}
