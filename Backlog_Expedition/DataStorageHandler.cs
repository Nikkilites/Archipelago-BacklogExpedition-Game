using Backlog_Expedition.Model;
using Newtonsoft.Json;

namespace Backlog_Expedition
{
    public class DataStorageHandler
    {
        public List<string> Regions { get; set; }
        public List<string> LocationNames { get; set; }
        public List<string> Items { get; set; }
        public List<string> Treasures { get; set; }
        public List<string> Monsters { get; set; }
        public List<string> Containers { get; set; }
        public List<string> Entities => Monsters.Concat(Containers).ToList();

        public StoryData StoryData { get; set; }

        public DataStorageHandler()
        {
            GameData rawData = LoadData<GameData>($"{Environment.CurrentDirectory}\\DataStorage\\data.json");

            Regions = ["Starting", .. rawData.extra_regions];
            Monsters = rawData.monsters;
            Containers = CreateContainerNames(rawData, Regions);
            LocationNames = CreateLocationNames(rawData, Regions, Containers);
            Items = CreateItemNames(rawData, Regions);
            Treasures = rawData.mcguffins;

            StoryData = LoadData<StoryData>($"{Environment.CurrentDirectory}\\DataStorage\\story.json");
        }

        private T LoadData<T>(string dataPath)
        {
            HelperMethods.Log($"Will load item data from {dataPath}");

            if (!File.Exists(dataPath))
                throw new FileNotFoundException("Failed to load Item data", dataPath);

            string json = File.ReadAllText(dataPath);
            T _data = JsonConvert.DeserializeObject<T>(json)
                ?? throw new Exception("Failed to parse JSON data.");

            return _data;
        }

        private static List<string> CreateContainerNames(GameData rawData, List<string> regions)
        {
            var containerNames = new List<string>();

            foreach (var container in rawData.containers)
            {
                foreach (var modifier in rawData.container_modifiers)
                {
                    containerNames.Add($"{modifier} {container}");
                }
            }

            return containerNames;
        }

        private static List<string> CreateLocationNames(GameData rawData, List<string> regions, List<string> containers)
        {
            var locationNames = new List<string>();

            foreach (var region in regions)
            {
                foreach (var monster in rawData.monsters)
                {
                    locationNames.Add($"Slay the {monster} in {region} Island");
                }

                foreach (var container in containers)
                {
                    locationNames.Add($"Opened the {container} in {region} Island");
                }
            }

            HelperMethods.Log("Loaded LocationNames");

            return locationNames;
        }

        private static List<string> CreateItemNames(GameData rawData, List<string> regions)
        {
            var ItemNames = new List<string>();

            foreach (var filler in rawData.fillers)
            {
                ItemNames.Add(filler);
            }

            foreach (var region in regions)
            {
                ItemNames.Add($"{region} Rune");
            }

            HelperMethods.Log("Loaded ItemNames");

            return ItemNames;
        }
    }
}
