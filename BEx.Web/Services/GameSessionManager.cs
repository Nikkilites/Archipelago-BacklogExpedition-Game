using BEx.Core;

namespace BEx.Web.Services
{
    public class GameSessionManager
    {
        private readonly Dictionary<Guid, GameSession> _sessions = new();

        public GameSession? GetSession(Guid sessionId)
        {
            if (_sessions.TryGetValue(sessionId, out var session))
                return session;

            return null;
        }

        public bool HasSession(Guid sessionId) => _sessions.ContainsKey(sessionId);

        public void RemoveSession(Guid sessionId) => _sessions.Remove(sessionId);


        public GameSession? CreateSession(
            string server,
            string player,
            string password,
            IServiceProvider services,
            Guid sessionId)
        {
            try
            {
                var logger = services.GetRequiredService<Core.ILogger>();
                var loader = services.GetRequiredService<IDataLoader>();
                var messages = services.GetRequiredService<IMessageService>();

                var session = new GameSession(logger, messages, loader);

                bool success = session.ConnectionHandler.Connect(server, player, password);

                if (!success)
                {
                    return null;
                }

                _sessions[sessionId] = session;

                Dictionary<string, object> slotData = session.ConnectionHandler.SlotData;
                session.GoalHandler.TreasuresToGoal = Convert.ToInt32(slotData["beaten_to_goal"]);
                //_session.RegionHandler.RunesRequired = Convert.ToInt32(slotData["runes_required"]);
                session.RegionHandler.CreateRegions(slotData);

                session.ItemHandler.SetupItemHandler();

                return session;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Session creation failed: {ex.Message}");
                return null;
            }
        }
    }
}
