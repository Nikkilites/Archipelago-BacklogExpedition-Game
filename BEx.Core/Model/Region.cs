namespace BEx.Core.Model
{
    public class Region(GameSession session, string name, string treasureName, List<Location> locations)
    {
        private GameSession _session { get; set; } = session;
        public string Name { get; private set; } = name;
        public string TreasureName { get; private set; } = treasureName;
        public List<Location> Locations { get; private set; } = locations;
        public string RuneName => $"{Name} Rune";
        public bool RuneReceived => Name == "Starting" || _session.ItemHandler.AvailableRunes[RuneName] >= _session.RegionHandler.RunesRequired;
        public int RuneCount => _session.ItemHandler.AvailableRunes[RuneName];
        public bool TreasureFound => 0 == Locations.Where(l => l.Entity == "monster").ToList().Count && RuneReceived;
        public string RuneAsciiFileName => $"rune_{Name.ToUpper()}".Replace(" ", "");
        public string RuneAsciiFileNameWText => $"{RuneAsciiFileName}_name";
        public string TreasureAsciiFileName => $"treasure_{TreasureName.ToUpper().Replace("'", "").Replace(" ", "")}";
        public void CheckLocation(Location location)
        {
            _session.ConnectionHandler.SendLocation(location.Id);
            Locations.Remove(location);
        }
    }
}
