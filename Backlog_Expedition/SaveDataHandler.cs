using Newtonsoft.Json;

namespace Backlog_Expedition
{
    public static class SaveDataHandler
    {
        private static readonly string roomInfoPath = $"{Environment.CurrentDirectory}\\DataStorage\\SaveData\\RoomInfo.json";

        public static RoomInfoData RoomInfo
        {
            get
            {
                return (RoomInfoData)LoadJson<RoomInfoData>(roomInfoPath);
            }
        }

        public static void SaveRoomInfo(string server, string player, string password)
        {
            string directoryPath = Path.GetDirectoryName(roomInfoPath);
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
                HelperMethods.Log($"Created directory: {directoryPath}");
            }

            HelperMethods.Log($"Will save Room Info to {directoryPath}");
            RoomInfoData roomInfoData = new(server, player, password);
            string json = JsonConvert.SerializeObject(roomInfoData, Formatting.Indented);
            File.WriteAllText(roomInfoPath, json);
        }

        public static object LoadJson<T>(string filepath)
        {
            if (!File.Exists(filepath))
            {
                throw new Exception($"File {filepath} not found.");
            }

            HelperMethods.Log($"Will load data from {filepath}");
            string json = File.ReadAllText(filepath);
            T data = JsonConvert.DeserializeObject<T>(json);
            return data;
        }
    }

    public class RoomInfoData
    {
        public string Server { get; set; }
        public string Playername { get; set; }
        public string Password { get; set; }

        public RoomInfoData(string server, string playername, string password)
        {
            Server = server;
            Playername = playername;
            if (password == "")
                Password = null;
            else
                Password = password;
        }
    }
}
