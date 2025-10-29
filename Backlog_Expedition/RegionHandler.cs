using Archipelago.MultiClient.Net.Models;
using Backlog_Expedition.Model;
using System.Collections.ObjectModel;
using System.Text.Json;

namespace Backlog_Expedition
{
    public class RegionHandler
    {
        public List<Region> Regions { get; set; } = [];

        public async Task CreateRegions(Dictionary<string, object> slotData)
        {
            List<Location> locations = await CreateLocations(slotData);

            List<Region> regions = [];

            List<string> regionNames = GameHandler.DataStorageHandler.Regions;
            List<string> mcGuffinNames = GameHandler.DataStorageHandler.Mcguffins;

            foreach (var (regionName, mcGuffinName) in regionNames.Zip(mcGuffinNames))
            {
                List<Location> regionLocations = [.. locations.Where(l => l.Region == regionName)];

                regions.Add(new Region(regionName, mcGuffinName, regionLocations));
            }

            Regions = regions;
        }

        private static async Task<List<Location>> CreateLocations(Dictionary<string, object> slotData)
        {
            HelperMethods.Log($"Will Process Hint Location Data");

            Dictionary<int, string> HintData = JsonSerializer.Deserialize<Dictionary<int, string>>(slotData["hint_data"].ToString());

            List<Location> locations = [];

            foreach (var kvp in HintData)
            {
                string name = GameHandler.ConnectionHandler.GetLocationNameFromId(kvp.Key);
                locations.Add(new Location(name, kvp.Key, kvp.Value));
            }

            ReadOnlyCollection<long> checkedLocationIds = GameHandler.ConnectionHandler.GetLocationsChecked();

            locations.RemoveAll(l => checkedLocationIds.Contains((long)l.Id));

            await ScoutLocations(locations);

            return locations;
        }

        private static async Task ScoutLocations(List<Location> locations)
        {
            Task<Dictionary<long, ScoutedItemInfo>> scoutedLocationsTask =
                GameHandler.ConnectionHandler.ScoutLocations(locations.Select(l => (long)l.Id).ToArray());

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
