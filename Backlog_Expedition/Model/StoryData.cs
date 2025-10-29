namespace Backlog_Expedition.Model
{
    public class StoryData(List<string> login, List<string> introduction, List<string> goal, Dictionary<string, List<string>> treasure_descriptions)
    {
        public List<string> login = login;
        public List<string> introduction = introduction;
        public List<string> goal = goal;
        public Dictionary<string, List<string>> open_chest = treasure_descriptions;
        public Dictionary<string, List<string>> slay_monster = treasure_descriptions;
        public Dictionary<string, List<string>> treasure_descriptions = treasure_descriptions;
    }
}
