namespace BEx.Core.Model
{
    public class Region
    {
        private GameSession _session { get; set; }
        public string Name { get; private set; }
        public string TreasureName { get; private set; }
        public List<Location> Locations { get; private set; }
        public string RuneName { get; private set; }
        public string RuneAsciiFileName { get; private set; }
        public string RuneAsciiFileNameWText { get; private set; }
        public string TreasureAsciiFileName { get; private set; }
        public bool RuneReceived => Name == "Starting" || _session.ItemHandler.AvailableRunes[RuneName] >= _session.RegionHandler.RunesRequired;
        public int RuneCount => _session.ItemHandler.AvailableRunes[RuneName];
        public bool TreasureFound => 0 == Locations.Where(l => l.Entity == "monster").ToList().Count && RuneReceived;

        public Region(GameSession session, string name, string treasureName, List<Location> locations)
        {
            _session = session;
            Name = name;
            TreasureName = treasureName;
            Locations = locations;

            RuneName = GetRuneName();
            RuneAsciiFileName = GetRuneAsciiFileName();
            RuneAsciiFileNameWText = GetRuneAsciiFileNameWText();
            TreasureAsciiFileName = GetTreasureAsciiFileName();
        }

        public void CheckLocation(Location location)
        {
            bool snapshot = TreasureFound;

            _session.ConnectionHandler.SendLocation(location.Id);
            Locations.Remove(location);

            if ((snapshot == false) && (TreasureFound == true))
            {
                _session.ConnectionHandler.SendMessage($"Logmundr: I have found the {TreasureName} of the {Name} Island! Onwards!");
            }
        }
        private string GetRuneName() => $"{Name} Rune";
        private string GetRuneAsciiFileName() => $"rune_{Name.ToUpper()}".Replace(" ", "");
        private string GetRuneAsciiFileNameWText() => $"{RuneAsciiFileName}_name";
        private string GetTreasureAsciiFileName() => $"treasure_{TreasureName.ToUpper().Replace("'", "").Replace(" ", "")}";
    }
}
