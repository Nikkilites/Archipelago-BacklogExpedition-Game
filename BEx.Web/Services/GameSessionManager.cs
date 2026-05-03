using BEx.Core;
using System.Collections.Concurrent;
using System.Diagnostics;
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
                entry.SetSessionActivity();
                return entry.Session;
            }

            return null;
        }

        public SessionEntry? GetSessionEntry(Guid sessionId)
        {
            if (_sessions.TryGetValue(sessionId, out var entry))
            {
                entry.SetSessionActivity();
                return entry;
            }

            return null;
        }

        public int GetSessionCount() => _sessions.Count;

        public bool HasSession(Guid sessionId) => _sessions.ContainsKey(sessionId);

        public async Task RemoveSession(Guid sessionId)
        {
            if (_sessions.TryRemove(sessionId, out var entry))
            {
                Console.WriteLine(
                    $"[CLEANUP]        {entry.Session.ConnectionHandler.PlayerName} was removed"
                );

                await entry.Session.ConnectionHandler.Disconnect();
            }
        }

        public async Task CleanupExpiredSessions()
        {
            var now = DateTime.UtcNow;
            var snapshot = _sessions.ToList();

            int removed = 0;

            foreach (var pair in snapshot)
            {
                var player = pair.Value.Session.ConnectionHandler.PlayerName;

                Console.WriteLine(
                    $"[CLEANUP] {player} was last seen {FormatTimeAgo(pair.Value.LastSeen)}"
                );

                if (now - pair.Value.LastSeen > TimeSpan.FromHours(2))
                {
                    Console.WriteLine(
                        $"[CLEANUP]        Removing inactive session: {player}"
                    );
                    await RemoveSession(pair.Key);
                    removed++;
                }
            }

            Console.WriteLine(
                $"[MEMORY] Sessions Active: {_sessions.Count} | Sessions Removed: {removed}"
            );
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
                var dataStorageHandler = services.GetRequiredService<DataStorageHandler>();
                var messages = services.GetRequiredService<IMessageService>();
                var textClient = new WebTextClient();

                var session = new GameSession(logger, textClient, messages, dataStorageHandler);

                bool success = session.ConnectionHandler.Connect(server, player, password);

                if (!success)
                {
                    return null;
                }

                session.ConnectionHandler.Disconnected += async () =>
                {
                    await RemoveSession(sessionId);
                };

                _sessions[sessionId] = new SessionEntry
                {
                    Session = session,
                    LastSeen = DateTime.UtcNow
                };

                Dictionary<string, object> slotData = session.ConnectionHandler.SlotData;

                if (slotData.TryGetValue("beaten_to_goal", out var beatVal))
                {
                    session.GoalHandler.TreasuresToGoal = Convert.ToInt32(beatVal);
                }
                else if (slotData.TryGetValue("treasures_to_goal", out var treVal))
                {
                    session.GoalHandler.TreasuresToGoal = Convert.ToInt32(treVal);
                }

                if (slotData.TryGetValue("runes_required", out var runeVal))
                {
                    session.RegionHandler.RunesRequired = Convert.ToInt32(runeVal);
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
                Console.WriteLine($"{player}'s session creation failed: {ex.Message}");
                return null;
            }
        }
    }

    public class SessionEntry
    {
        public GameSession Session { get; set; } = default!;
        public DateTime LastSeen { get; set; } = DateTime.UtcNow;
        public List<Notification> RecentLocationsSent { get; } = new();

        public void SetSessionActivity()
        {
            LastSeen = DateTime.UtcNow;
        }
    }
}
