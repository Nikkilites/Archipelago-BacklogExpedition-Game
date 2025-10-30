using Archipelago.MultiClient.Net.Models;

namespace Backlog_Expedition.Model
{
    public class Location(string name, int id, string hint)
    {
        public string Name { get; set; } = name;
        public int Id { get; set; } = id;
        public string Hint { get; set; } = hint;
        public ScoutedItemInfo? ScoutedInfo { get; set; } = null;
        public string Region => Name[(Name.LastIndexOf(" in ") + 4)..]
            .Replace(" Island", "");
        public string Entity => Name.StartsWith("Slay the ") ? "monster" : "container";
        public string EntityName
        {
            get
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
        public string AsciiFileName => $"{Entity}_{EntityName.ToUpper()}";
    }
}
