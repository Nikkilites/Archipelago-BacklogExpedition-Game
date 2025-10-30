namespace Backlog_Expedition
{
    public static class HelperMethods
    {
        private static readonly object logLock = new();

        public static void Log(string logText)
        {
            lock (logLock)
            {
                using (StreamWriter log = File.Exists("logfile.txt")
                    ? File.AppendText("logfile.txt")
                    : new StreamWriter("logfile.txt"))
                {
                    log.WriteLine($"[{DateTime.Now:HH:mm:ss}] {logText}");
                }
            }
        }

        public static void ClearLog()
        {
            lock (logLock)
            {
                File.WriteAllText("logfile.txt", string.Empty);
            }
        }
    }
}
