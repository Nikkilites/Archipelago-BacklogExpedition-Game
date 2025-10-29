namespace Backlog_Expedition.Model
{
    public class Region(string name, string mcGuffinName, List<Location> locations)
    {
        public string Name { get; private set; } = name;
        public string McGuffinName { get; private set; } = mcGuffinName;
        public List<Location> Locations { get; private set; } = locations;
        public string RuneName => $"{Name} Rune";
        public bool RuneReceived => Name == "Starting" || GameHandler.ItemHandler.AvailableRunes.Exists(n => n == RuneName);
        public bool McGuffinFound => 0 == Locations.Where(l => l.Entity == "monster").ToList().Count;
        public string RuneAsciiFileName => $"rune_{Name.ToUpper()}".Replace(" ", "");
        public string McGuffinAsciiFileName => $"mcguffin_{McGuffinName.ToUpper().Replace("'", "").Replace(" ", "")}";
        public void CheckLocation(Location location)
        {
            HelperMethods.Log($"Send Location: {location.Name}");

            GameHandler.ConnectionHandler.SendLocation(location.Id);
            Locations.Remove(location);
        }
    }
}
