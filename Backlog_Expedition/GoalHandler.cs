namespace Backlog_Expedition
{
    public class GoalHandler
    {
        public int BeatenToGoal { get; set; } = 0;
        public void CheckIfGoal()
        {
            int treasuresFound = GameHandler.RegionHandler.Regions.Where(r => r.TreasureFound == true).ToList().Count();
            if (treasuresFound == BeatenToGoal)
                
                OnGoalConditionMet();
        }

        private void OnGoalConditionMet()
        {
            GameHandler.ConnectionHandler.SendGoal();
            ScreenHandler.PrintGoalScreen(GameHandler.DataStorageHandler.StoryData.goal);
        }
    }
}
