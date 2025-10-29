namespace Backlog_Expedition
{
    public static class Updater
    {
        public static event Action? OnFrameUpdated;

        private static CancellationTokenSource? _cts;

        public static void Start()
        {
            _cts = new CancellationTokenSource();
            _ = RunAsync(_cts.Token);
        }

        public static void Stop()
        {
            _cts?.Cancel();
        }

        private static async Task RunAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    OnFrameUpdated?.Invoke();

                    await Task.Delay(33, token);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    HelperMethods.Log($"[Updater] Error during update: {ex.Message}");
                }
            }
        }
    }
}
