namespace Backlog_Expedition
{
    public class GoalHandler
    {
        public int TreasuresToGoal { get; set; } = 0;
        public int TreasuresFound => GameHandler.RegionHandler.Regions.Where(r => r.TreasureFound == true).ToList().Count();
        public void CheckIfGoal()
        {
            if (TreasuresFound == TreasuresToGoal)
                
                OnGoalConditionMet();
        }

        private void OnGoalConditionMet()
        {
            GameHandler.ConnectionHandler.SendGoal();
            ScreenHandler.PrintGoalScreen(GameHandler.DataStorageHandler.StoryData.goal);
        }
    }
}
