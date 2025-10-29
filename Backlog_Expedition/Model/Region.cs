namespace Backlog_Expedition.Model
{
    public class Region(string name, string treasureName, List<Location> locations)
    {
        public string Name { get; private set; } = name;
        public string TreasureName { get; private set; } = treasureName;
        public List<Location> Locations { get; private set; } = locations;
        public string RuneName => $"{Name} Rune";
        public bool RuneReceived => Name == "Starting" || GameHandler.ItemHandler.AvailableRunes.Exists(n => n == RuneName);
        public bool TreasureFound => 0 == Locations.Where(l => l.Entity == "monster").ToList().Count;
        public string RuneAsciiFileName => $"rune_{Name.ToUpper()}".Replace(" ", "");
        public string RuneAsciiFileNameWText => $"{RuneAsciiFileName}_name";
        public string TreasureAsciiFileName => $"mcguffins_{TreasureName.ToUpper().Replace("'", "").Replace(" ", "")}";
        public void CheckLocation(Location location)
        {
            HelperMethods.Log($"Send Location: {location.Name}");

            GameHandler.ConnectionHandler.SendLocation(location.Id);
            Locations.Remove(location);
        }
    }
}
