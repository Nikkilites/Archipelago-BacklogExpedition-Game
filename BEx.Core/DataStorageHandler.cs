using BEx.Core.Model;

namespace BEx.Core
{
    public class DataStorageHandler
    {
        public IReadOnlyList<string> Regions { get; }
        public IReadOnlyList<string> LocationNames { get; }
        public IReadOnlyList<string> Items { get; }
        public IReadOnlyList<string> Treasures { get; }
        public IReadOnlyList<string> Monsters { get; }
        public IReadOnlyList<string> Containers { get; }
        public IReadOnlyList<string> Entities { get; }
        public StoryData StoryData { get; }

        public DataStorageHandler(IDataLoader loader)
        {
            GameData rawData = loader.LoadData();

            var regions = new List<string> { "Starting" };
            regions.AddRange(rawData.extra_regions);

            Regions = regions.AsReadOnly();

            Monsters = rawData.monsters;
            Containers = CreateContainerNames(rawData);
            LocationNames = CreateLocationNames(rawData);
            Items = CreateItemNames(rawData);
            Treasures = rawData.mcguffins;
            Entities = GetEntities();

            StoryData = loader.LoadStory();
        }

        private List<string> GetEntities() => Monsters.Concat(Containers).ToList();

        private List<string> CreateContainerNames(GameData rawData)
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

        private List<string> CreateLocationNames(GameData rawData)
        {
            var locationNames = new List<string>();

            foreach (var region in Regions)
            {
                foreach (var monster in rawData.monsters)
                {
                    locationNames.Add($"Slay the {monster} in {region} Island");
                }

                foreach (var container in Containers)
                {
                    locationNames.Add($"Opened the {container} in {region} Island");
                }
            }

            return locationNames;
        }

        private List<string> CreateItemNames(GameData rawData)
        {
            var ItemNames = new List<string>();

            foreach (var filler in rawData.fillers)
            {
                ItemNames.Add(filler);
            }

            foreach (var region in Regions)
            {
                ItemNames.Add($"{region} Rune");
            }

            return ItemNames;
        }
    }
}
