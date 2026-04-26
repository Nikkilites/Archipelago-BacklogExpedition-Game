using BEx.Core;
using System.Collections.Concurrent;
using static BEx.Web.Components.Pages.Game;

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

        public SessionEntry? GetSessionEntry(Guid sessionId)
        {
            if (_sessions.TryGetValue(sessionId, out var entry))
            {
                entry.LastSeen = DateTime.UtcNow;
                return entry;
            }

            return null;
        }

        public bool HasSession(Guid sessionId) => _sessions.ContainsKey(sessionId);

        public async Task RemoveSession(Guid sessionId)
        {
            if (_sessions.TryRemove(sessionId, out var entry))
            {
                await entry.Session.ConnectionHandler.Disconnect();
            }
        }

        public async Task CleanupExpiredSessions()
        {
            var now = DateTime.UtcNow;
            var snapshot = _sessions.ToList();

            foreach (var pair in snapshot)
            {
                var player = pair.Value.Session.ConnectionHandler.PlayerName;

                Console.WriteLine(
                    $"{player} was last seen {FormatTimeAgo(pair.Value.LastSeen)}"
                );

                if (now - pair.Value.LastSeen > TimeSpan.FromHours(3))
                {
                    Console.WriteLine(
                        $"[CLEANUP] {player} removed (last seen {FormatTimeAgo(pair.Value.LastSeen)})"
                    );
                    await RemoveSession(pair.Key);
                }
            }
        }

        private static string FormatTimeAgo(DateTime lastSeen)
        {
            var diff = DateTime.UtcNow - lastSeen;

            if (diff.TotalSeconds < 60)
                return $"{(int)diff.TotalSeconds}s ago";

            if (diff.TotalMinutes < 60)
                return $"{(int)diff.TotalMinutes}m ago";

            if (diff.TotalHours < 24)
                return $"{(int)diff.TotalHours}h ago";

            return $"{(int)diff.TotalDays}d ago";
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
                var textClient = new WebTextClient();

                var session = new GameSession(logger, textClient, messages, loader);

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
                if (slotData.TryGetValue("runes_required", out var value))
                {
                    session.RegionHandler.RunesRequired = Convert.ToInt32(value);
                }
                else
                {
                    session.RegionHandler.RunesRequired = 1;
                }

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
        public List<Notification> RecentLocationsSent { get; } = new();
    }
}
