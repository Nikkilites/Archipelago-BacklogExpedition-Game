namespace Backlog_Expedition.Model
{
    public class StoryData(List<string> login, List<string> introduction, List<string> goal, Dictionary<string, List<string>> treasure_descriptions, Dictionary<string, string> open_chest, Dictionary<string, string> slay_monster)
    {
        public List<string> login = login;
        public List<string> introduction = introduction;
        public List<string> goal = goal;
        public Dictionary<string, string> open_chest = open_chest;
        public Dictionary<string, string> slay_monster = slay_monster;
        public Dictionary<string, List<string>> treasure_descriptions = treasure_descriptions;
    }
}
