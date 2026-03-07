namespace BEx.Core.Model
{
    public class StoryData
    {
        public List<string> login { get; set; } = new();
        public List<string> introduction { get; set; } = new();
        public List<string> goal { get; set; } = new();
        public Dictionary<string, List<string>> treasure_descriptions { get; set; } = new();
        public Dictionary<string, string> open_chest { get; set; } = new();
        public Dictionary<string, string> slay_monster { get; set; } = new();
    }
}
