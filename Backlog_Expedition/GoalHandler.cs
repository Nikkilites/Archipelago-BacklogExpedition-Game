namespace Backlog_Expedition
{
    public class GoalHandler
    {
        public int BeatenToGoal { get; set; } = 0;
        public void CheckIfGoal()
        {
            if (GameHandler.RegionHandler.Regions.Where(r => r.McGuffinFound == true).Count() == BeatenToGoal)
                OnGoalConditionMet();
        }

        private void OnGoalConditionMet()
        {
            GameHandler.ConnectionHandler.SendGoal();
        }
    }
}
