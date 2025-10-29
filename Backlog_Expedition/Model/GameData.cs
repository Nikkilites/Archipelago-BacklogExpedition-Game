namespace Backlog_Expedition.Model
{
    public class GameData
    {
        public List<string> extra_regions;
        public List<string> monsters;
        public List<string> container_modifiers;
        public List<string> containers;
        public List<string> fillers;
        public List<string> mcguffins;

        public GameData(List<string> extra_regions, List<string> monsters, List<string> container_modifiers, List<string> containers, List<string> fillers, List<string> mcguffins)
        {
            this.extra_regions = extra_regions;
            this.monsters = monsters;
            this.container_modifiers = container_modifiers;
            this.containers = containers;
            this.fillers = fillers;
            this.mcguffins = mcguffins;
        }
    }
}
