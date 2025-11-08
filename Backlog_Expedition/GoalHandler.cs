namespace Backlog_Expedition
{
    public class GoalHandler
    {
        public int TreasuresToGoal { get; set; } = 0;
        public void CheckIfGoal()
        {
            int treasuresFound = GameHandler.RegionHandler.Regions.Where(r => r.TreasureFound == true).ToList().Count();
            if (treasuresFound == TreasuresToGoal)
                
                OnGoalConditionMet();
        }

        private void OnGoalConditionMet()
        {
            GameHandler.ConnectionHandler.SendGoal();
            ScreenHandler.PrintGoalScreen(GameHandler.DataStorageHandler.StoryData.goal);
        }
    }
}
