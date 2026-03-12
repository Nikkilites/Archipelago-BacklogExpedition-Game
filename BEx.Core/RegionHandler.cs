using Archipelago.MultiClient.Net.Models;
using BEx.Core.Model;
using System.Collections.ObjectModel;
using System.Text.Json;

namespace BEx.Core
{
    public class RegionHandler(GameSession gameSession, ILogger logger)
    {
        private readonly GameSession _gameSession = gameSession;
        private readonly ILogger _logger = logger;

        public List<Region> Regions { get; set; } = [];
        public List<Region> AvailableRegions => Regions.Where(r => r.RuneReceived).ToList();
        public List<Region> RegionsWithARune => Regions.Where(r => r.RuneCount >= 1).ToList();
        public List<Region> RegionsWithFoundTreasure => Regions.Where(r => r.TreasureFound).ToList();
        public List<Region> RegionsInWorld => Regions.Where(r => r.RuneReceived || r.Locations.Count() != 0).ToList();

        public int RunesRequired { get; set; } = 1;

        public async Task CreateRegions(Dictionary<string, object> slotData)
        {
            List<Location> locations = await CreateLocations(slotData);

            List<Region> regions = [];

            List<string> regionNames = _gameSession.DataStorageHandler.Regions;
            List<string> treasureNames = _gameSession.DataStorageHandler.Treasures;

            foreach (var (regionName, treasureName) in regionNames.Zip(treasureNames))
            {
                List<Location> regionLocations = [.. locations.Where(l => l.Region == regionName)];

                regions.Add(new Region(gameSession, regionName, treasureName, regionLocations));
            }

            Regions = regions;
        }

        private async Task<List<Location>> CreateLocations(Dictionary<string, object> slotData)
        {
            _logger.Log($"Will Process Hint Location Data");

            Dictionary<int, string> HintData = JsonSerializer.Deserialize<Dictionary<int, string>>(slotData["hint_data"].ToString());

            List<Location> locations = [];

            foreach (var kvp in HintData)
            {
                string name = _gameSession.ConnectionHandler.GetLocationNameFromId(kvp.Key);
                locations.Add(new Location(name, kvp.Key, kvp.Value));
            }

            ReadOnlyCollection<long> checkedLocationIds = _gameSession.ConnectionHandler.GetLocationsChecked();

            locations.RemoveAll(l => checkedLocationIds.Contains((long)l.Id));

            await ScoutLocations(locations);

            return locations;
        }

        private async Task ScoutLocations(List<Location> locations)
        {
            Task<Dictionary<long, ScoutedItemInfo>> scoutedLocationsTask =
                _gameSession.ConnectionHandler.ScoutLocations(locations.Select(l => (long)l.Id).ToArray());

            Dictionary<long, ScoutedItemInfo> scoutedLocations = await scoutedLocationsTask;

            foreach (var location in locations)
            {
                if (scoutedLocations.TryGetValue(location.Id, out ScoutedItemInfo info))
                {
                    location.ScoutedInfo = info;
                }
            }
        }
    }
}
