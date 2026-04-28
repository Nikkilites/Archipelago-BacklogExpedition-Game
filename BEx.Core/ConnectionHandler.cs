using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.MessageLog.Messages;
using Archipelago.MultiClient.Net.Models;
using Archipelago.MultiClient.Net.Packets;
using System.Collections.ObjectModel;

namespace BEx.Core
{
    public class ConnectionHandler
    {
        private readonly GameSession _gameSession;
        private readonly ILogger _logger;
        private readonly ITextClient _textClient;
        public event Func<Task>? Disconnected;

        public ConnectionHandler(GameSession session, ILogger logger, ITextClient textClient)
        {
            _gameSession = session;
            _logger = logger;
            _textClient = textClient;
        }

        private ArchipelagoSession session;
        private const string gameName = "Backlog Expedition";
        public string PlayerName = "";
        public Dictionary<string, object> SlotData { get; private set; }
        public bool Connected { get; private set; }

        public ConnectionHandler() { }

        public bool Connect(string server, string player, string pass)
        {
            _logger.Log($"[ARCHIPELAGO] Will try to connect to server with server: {server}, player: {player}, password: {pass}");

            LoginResult result;

            try
            {
                session = ArchipelagoSessionFactory.CreateSession(server);
                session.Items.ItemReceived += _gameSession.ItemHandler.OnItemReceived;
                session.Socket.SocketClosed += OnDisconnect;
                session.Socket.ErrorReceived += OnError;
                session.MessageLog.OnMessageReceived += OnMessageReceived;
                result = session.TryConnectAndLogin(gameName, player, ItemsHandlingFlags.AllItems, password: pass, requestSlotData: true);
            }
            catch (Exception e)
            {
                result = new LoginFailure(e.GetBaseException().Message);
            }

            if (!result.Successful)
            {
                LoginFailure failure = (LoginFailure)result;
                string errorMessage = $"[ARCHIPELAGO] Failed to Connect to {server} as {player}:";
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

            _logger.Log($"[ARCHIPELAGO] Successfully connected to {server} as {player}.");

            return true;
        }

        public async Task Disconnect()
        {
            if (Connected)
            {
                Connected = false;

                session.Items.ItemReceived -= _gameSession.ItemHandler.OnItemReceived; 
                session.Socket.SocketClosed -= OnDisconnect; 
                session.Socket.ErrorReceived -= OnError; 
                session.MessageLog.OnMessageReceived -= OnMessageReceived;

                await session.Socket.DisconnectAsync();

                session = null;
                SlotData = null;
            }
        }

        public async void OnDisconnect(string reason)
        {
            if (Connected)
            {
                reason += $"\n[ARCHIPELAGO]    Called from OnDisconnect";
                _logger.Log($"[ARCHIPELAGO] {_gameSession.ConnectionHandler.PlayerName} Disconnected {reason}");
                Connected = false;
                await Disconnect();


                if (Disconnected != null)
                    await Disconnected.Invoke();
            }
        }

        public async void OnError(Exception e, string message)
        {
            if (Connected)
            {
                message += $"\n[ARCHIPELAGO]    Called from OnError";
                _logger.Log($"[ARCHIPELAGO] {_gameSession.ConnectionHandler.PlayerName} Disconnected {message}");
                Connected = false;
                await Disconnect();


                if (Disconnected != null)
                    await Disconnected.Invoke();
            }
        }

        public async void SendLocation(long apId)
        {
            if (!Connected)
            {
                return;
            }

            await session.Locations.CompleteLocationChecksAsync(apId);
        }

        public async void SendLocations(List<string> locations)
        {
            if (!Connected)
            {
                return;
            }

            long[] apIds = [.. locations.Select(x => session.Locations.GetLocationIdFromName(gameName, x))];

            await session.Locations.CompleteLocationChecksAsync(apIds);
        }

        public void SendGoal()
        {
            var statusUpdatePacket = new StatusUpdatePacket
            {
                Status = ArchipelagoClientState.ClientGoal
            };

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
            session.Hints.CreateHints(HintStatus.Unspecified, id);
        }

        public Hint[] GetHints()
        {
            return session.Hints.GetHints(GetThisSlotId());
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
            _logger.Log($"[ARCHIPELAGO] Update Server Data Storage {key} to {value}");
            session.DataStorage[key] = value;
        }

        public void SendMessage(string message)
        {
            try
            {
                if (!Connected)
                {
                    _textClient.ShowMessage($"Failed to send message due to disconnect.");
                    return;
                }
                session.Socket.SendPacketAsync(new SayPacket() { Text = message });
            }
            catch (Exception e)
            {
                _logger.Log($"[ARCHIPELAGO] Error when sending message {e}");
            }
        }

        public void OnMessageReceived(LogMessage message)
        {
            try
            {
                _textClient.ShowMessage(message.ToString());
            }
            catch (Exception e)
            {
                _logger.Log($"[ARCHIPELAGO] Error when parsing received message {e}");
            }
        }
    }
}
