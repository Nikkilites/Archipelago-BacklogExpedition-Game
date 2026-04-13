namespace BEx.Core.Model
{
    public class GameData
    {
        public List<string> extra_regions { get; set; } = [];
        public List<string> monsters { get; set; } = [];
        public List<string> container_modifiers { get; set; } = [];
        public List<string> containers { get; set; } = [];
        public List<string> fillers { get; set; } = [];
        public List<string> mcguffins { get; set; } = [];
    }
}
