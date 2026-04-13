using BEx.Core.Model;

namespace BEx.Core
{
    public class DataStorageHandler
    {
        private readonly ILogger _logger;
        private readonly IDataLoader _loader;

        public List<string> Regions { get; set; }
        public List<string> LocationNames { get; set; }
        public List<string> Items { get; set; }
        public List<string> Treasures { get; set; }
        public List<string> Monsters { get; set; }
        public List<string> Containers { get; set; }
        public List<string> Entities => Monsters.Concat(Containers).ToList();
        public StoryData StoryData { get; set; }

        public DataStorageHandler(ILogger logger, IDataLoader loader)
        {
            _logger = logger;
            _loader = loader;

            GameData rawData = _loader.LoadData();

            Regions = new List<string> { "Starting" };
            Regions.AddRange(rawData.extra_regions);

            Monsters = rawData.monsters;
            Containers = CreateContainerNames(rawData, Regions);
            LocationNames = CreateLocationNames(rawData, Regions, Containers);
            Items = CreateItemNames(rawData, Regions);
            Treasures = rawData.mcguffins;

            StoryData = _loader.LoadStory();

            _logger.Log("Loaded Data Storage into Session");
        }

        private List<string> CreateContainerNames(GameData rawData, List<string> regions)
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

        private List<string> CreateLocationNames(GameData rawData, List<string> regions, List<string> containers)
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

            return locationNames;
        }

        private List<string> CreateItemNames(GameData rawData, List<string> regions)
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

            return ItemNames;
        }
    }
}
