using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Models;
using Archipelago.MultiClient.Net.Packets;
using System.Collections.ObjectModel;

namespace BEx.Core
{
    public class ConnectionHandler
    {
        private readonly GameSession _gamesession;
        private readonly ILogger _logger;

        public ConnectionHandler(GameSession session, ILogger logger)
        {
            _gamesession = session;
            _logger = logger;
        }

        private ArchipelagoSession session;
        private const string gameName = "Backlog Expedition";
        public string PlayerName = "";
        public Dictionary<string, object> SlotData { get; private set; }
        public bool Connected { get; private set; }

        public ConnectionHandler() { }

        public bool Connect(string server, string player, string pass)
        {
            _logger.Log($"Will try to connect to server with {server}, {player}, {pass}.");

            LoginResult result;

            try
            {
                session = ArchipelagoSessionFactory.CreateSession(server);
                session.Items.ItemReceived += _gamesession.ItemHandler.OnItemReceived;
                session.Socket.SocketClosed += OnDisconnect;
                session.Socket.ErrorReceived += OnError;
                result = session.TryConnectAndLogin(gameName, player, ItemsHandlingFlags.AllItems, password: pass, requestSlotData: true);
            }
            catch (Exception e)
            {
                result = new LoginFailure(e.GetBaseException().Message);
            }

            if (!result.Successful)
            {
                LoginFailure failure = (LoginFailure)result;
                string errorMessage = $"Failed to Connect to {server} as {player}:";
                foreach (string error in failure.Errors)
                {
                    errorMessage += $"\n    {error}";
                }
                foreach (ConnectionRefusedError error in failure.ErrorCodes)
                {
                    errorMessage += $"\n    {error}";
                }

                _logger.Log(errorMessage);
                return false;
            }

            Connected = true;
            LoginSuccessful loginSuccess = (LoginSuccessful)result;

            SlotData = loginSuccess.SlotData;

            PlayerName = player;

            _logger.Log($"Successfully connected to {server}.");

            return true;
        }

        public async Task Disconnect()
        {
            session = null;
            SlotData = null;
            if (Connected)
            {
                await session.Socket.DisconnectAsync();
                Connected = false;
            }
        }

        public async void OnDisconnect(string reason)
        {
            if (Connected)
            {
                reason += $"\n    Called from OnDisconnect";
                _logger.Log($"Disconnected {reason}");
                Connected = false;
                await Disconnect();
            }
        }

        public async void OnError(Exception e, string message)
        {
            if (Connected)
            {
                message += $"\n    Called from OnError";
                _logger.Log($"Disconnected {message}");
                Connected = false;
                await Disconnect();
            }
        }

        public async void SendLocation(long apId)
        {
            if (!Connected)
            {
                return;
            }

            _logger.Log($"Sending location with id: {apId} to server");

            await session.Locations.CompleteLocationChecksAsync(apId);

            _logger.Log($"Location {apId} sent successfully.");
        }

        public async void SendLocations(List<string> locations)
        {
            if (!Connected)
            {
                return;
            }

            long[] apIds = [.. locations.Select(x => session.Locations.GetLocationIdFromName(gameName, x))];

            _logger.Log($"Sending locations with ids: {string.Join(", ", apIds)} to server.");

            await session.Locations.CompleteLocationChecksAsync(apIds);

            _logger.Log($"Locations {string.Join(", ", apIds)} sent successfully.");
        }

        public void SendGoal()
        {
            var statusUpdatePacket = new StatusUpdatePacket
            {
                Status = ArchipelagoClientState.ClientGoal
            };

            _logger.Log($"Sending goal to server");

            session.Socket.SendPacket(statusUpdatePacket);
        }

        public ReadOnlyCollection<long> GetLocationsChecked() => session.Locations.AllLocationsChecked;
        public int AllLocationsCount => session.Locations.AllLocations.Count;

        public int GetThisSlotId()
        {
            return session.ConnectionInfo.Slot;
        }

        public string GetPlayerNameFromSlot(int slot)
        {
            return session.Players.GetPlayerName(slot) ?? "Server";
        }

        public string GetPlayerAliasFromSlot(int slot)
        {
            return session.Players.GetPlayerAlias(slot) ?? "Server";
        }

        public string GetItemNameFromId(long id)
        {
            return session.Items.GetItemName(id) ?? $"Item[{id}]";
        }

        public string GetLocationNameFromId(long id)
        {
            return session.Locations.GetLocationNameFromId(id) ?? $"Location[{id}]";
        }

        public void SendLocationHint(long id)
        {
            _logger.Log($"Send location hint for location with id: {id} to server");
            session.Hints.CreateHints(HintStatus.Unspecified, id);
        }

        public Hint[] GetHints()
        {
            return session.Hints.GetHints();
        }

        public async Task<Dictionary<long, ScoutedItemInfo>> ScoutLocations(long[] ids)
        {
            return await session.Locations.ScoutLocationsAsync(ids);
        }

        public int GetServerDataStorage(string key)
        {
            return session.DataStorage[key];
        }

        public void UpdateServerDataStorage(string key, int value)
        {
            _logger.Log($"Update Server Data Storage {key} to {value}");
            session.DataStorage[key] = value;
        }
    }
}
