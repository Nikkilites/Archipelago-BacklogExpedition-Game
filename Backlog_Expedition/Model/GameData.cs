namespace Backlog_Expedition.Model
{
    public class GameData(List<string> extra_regions, List<string> monsters, List<string> container_modifiers, List<string> containers, List<string> fillers, List<string> mcguffins)
    {
        public List<string> extra_regions = extra_regions;
        public List<string> monsters = monsters;
        public List<string> container_modifiers = container_modifiers;
        public List<string> containers = containers;
        public List<string> fillers = fillers;
        public List<string> mcguffins = mcguffins;
    }
}
