namespace BEx.Core
{
    public class GoalHandler(GameSession gameSession, ILogger logger)
    {
        private readonly GameSession _gameSession = gameSession;
        private readonly ILogger _logger = logger;

        public int TreasuresToGoal { get; set; } = 0;
        public int TreasuresFound => _gameSession.RegionHandler.Regions.Count(r => r.TreasureFound);
        public event Action? GoalReached;
        public void CheckIfGoal()
        {
            if (TreasuresFound == TreasuresToGoal)
                OnGoalConditionMet();
        }

        private void OnGoalConditionMet()
        {
            _logger.Log("Goal reached");

            _gameSession.ConnectionHandler.SendGoal();
            List<string> goalTexts =
                _gameSession.DataStorageHandler.StoryData.goal;

            GoalReached?.Invoke();
        }
    }
}
