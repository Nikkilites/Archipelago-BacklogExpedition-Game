using Archipelago.MultiClient.Net.Models;

namespace BEx.Core.Model
{
    public class Location
    {
        public string Name { get; private set; }
        public int Id { get; private set; }
        public string Hint { get; private set; }
        public ScoutedItemInfo? ScoutedInfo { get; set; } = null;
        public string Region { get; private set; }
        public string Entity { get; private set; }
        public string EntityName { get; private set; }
        public string AsciiFileName { get; private set; }
        public bool IsChecked { get; set; } = false;

        public Location(string name, int id, string hint)
        {
            Name = name;
            Id = id;
            Hint = hint;

            Region = GetRegion();
            Entity = GetEntity();
            EntityName = GetEntityName();
            AsciiFileName = GetAsciiFileName();
        }

        private string GetAsciiFileName() => $"{Entity}_{EntityName.ToUpper()}";
        private string GetRegion() => Name[(Name.LastIndexOf(" in ") + 4)..].Replace(" Island", "");
        private string GetEntity() => Name.StartsWith("Slay the ") ? "monster" : "container";
        private string GetEntityName()
        {
            int start = Name.IndexOf("the ", StringComparison.OrdinalIgnoreCase);
            if (start < 0) return string.Empty;
            start += "the ".Length;

            int end = Name.IndexOf(" in ", start, StringComparison.OrdinalIgnoreCase);

            if (end < 0) return string.Empty;

            string tmpName = Name[start..end].Trim();

            if (Entity == "container")
                tmpName = tmpName.Split(" ")[1];

            return tmpName;
        }
    }
}
