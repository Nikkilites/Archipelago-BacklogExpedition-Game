using Newtonsoft.Json;

namespace Backlog_Expedition
{
    public static class HelperMethods
    {
        public static object LoadJson<T>(string filepath)
        {
            if (!File.Exists(filepath))
            {
                throw new Exception($"File {filepath} not found.");
            }

            Log($"Will load data from {filepath}");
            string json = File.ReadAllText(filepath);
            T data = JsonConvert.DeserializeObject<T>(json);
            Log($"Data {data} loaded from {filepath}");
            return data;
        }

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
