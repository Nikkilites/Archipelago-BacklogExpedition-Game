namespace BEx.Core
{
    public class GoalHandler(GameSession gameSession)
    {
        private readonly GameSession _gameSession = gameSession;

        public int TreasuresToGoal { get; set; } = 0;
        public int TreasuresFound => _gameSession.RegionHandler.Regions.Count(r => r.TreasureFound);
        public event Action? GoalReached;
        private bool _goalHasBeenSent = false;
        public bool CheckIfGoal()
        {
            if (TreasuresFound == TreasuresToGoal & !_goalHasBeenSent)
            {
                OnGoalConditionMet();
                return true;
            }
            else
            {
                return false;
            }
        }

        private void OnGoalConditionMet()
        {
            _goalHasBeenSent = true;

            _gameSession.Logger.Log($"{_gameSession.ConnectionHandler.PlayerName} reached goal!");

            _gameSession.ConnectionHandler.SendGoal();

            GoalReached?.Invoke();
        }
    }
}
