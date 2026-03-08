using BEx.Core;
using System.Collections.Concurrent;

namespace BEx.Web.Services
{
    public class GameSessionManager
    {
        private readonly ConcurrentDictionary<Guid, SessionEntry> _sessions = new();

        public GameSession? GetSession(Guid sessionId)
        {
            if (_sessions.TryGetValue(sessionId, out var entry))
            {
                entry.LastSeen = DateTime.UtcNow;
                return entry.Session;
            }

            return null;
        }

        public bool HasSession(Guid sessionId) => _sessions.ContainsKey(sessionId);

        public async void RemoveSession(Guid sessionId)
        {
            if (_sessions.TryRemove(sessionId, out var entry))
            {
                await entry.Session.ConnectionHandler.Disconnect();
            }
        }

        public void CleanupExpiredSessions()
        {
            var now = DateTime.UtcNow;

            foreach (var pair in _sessions)
            {
                if (now - pair.Value.LastSeen > TimeSpan.FromMinutes(5))
                {
                    RemoveSession(pair.Key);
                }
            }
        }

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

                _sessions[sessionId] = new SessionEntry
                {
                    Session = session,
                    LastSeen = DateTime.UtcNow
                };

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

    public class SessionEntry
    {
        public GameSession Session { get; set; } = default!;
        public DateTime LastSeen { get; set; } = DateTime.UtcNow;
    }
}
