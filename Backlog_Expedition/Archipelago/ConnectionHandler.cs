using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Models;
using Archipelago.MultiClient.Net.Packets;
using System.Collections.ObjectModel;

namespace Backlog_Expedition.Archipelago
{
    public class ConnectionHandler
    {
        private ArchipelagoSession session;
        private const string gameName = "Backlog Expedition";
        public Dictionary<string, object> SlotData { get; private set; }
        public bool Connected { get; private set; }

        public ConnectionHandler() { }

        public bool Connect(string server, string player, string pass)
        {
            HelperMethods.Log($"Will try to connect to server with {server}, {player}, {pass}.");

            LoginResult result;

            try
            {
                session = ArchipelagoSessionFactory.CreateSession(server);
                session.Items.ItemReceived += GameHandler.ItemHandler.OnItemReceived;
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

                HelperMethods.Log(errorMessage);
                return false;
            }

            Connected = true;
            LoginSuccessful loginSuccess = (LoginSuccessful)result;

            SlotData = loginSuccess.SlotData;

            HelperMethods.Log($"Successfully connected to {server}.");

            return true;
        }

        public void OnDisconnect(string reason)
        {
            if (Connected)
            {
                Connected = false;
                session = null;
                HelperMethods.Log($"Disconnected {reason}");
                Updater.Stop();
            }
        }

        public void OnError(Exception e, string message)
        {
            message += $"\n    Called from OnError";
            HelperMethods.Log($"Disconnected {message}");
            throw e;
        }

        public async void SendLocation(long apId)
        {
            if (!Connected)
            {
                return;
            }

            HelperMethods.Log($"Sending location with id: {apId} to server");

            await session.Locations.CompleteLocationChecksAsync(apId);

            HelperMethods.Log($"Location {apId} sent successfully.");
        }

        public async void SendLocations(List<string> locations)
        {
            if (!Connected)
            {
                return;
            }

            long[] apIds = [.. locations.Select(x => session.Locations.GetLocationIdFromName(gameName, x))];

            HelperMethods.Log($"Sending locations with ids: {string.Join(", ", apIds)} to server.");

            await session.Locations.CompleteLocationChecksAsync(apIds);

            HelperMethods.Log($"Locations {string.Join(", ", apIds)} sent successfully.");
        }

        public void SendGoal()
        {
            var statusUpdatePacket = new StatusUpdatePacket
            {
                Status = ArchipelagoClientState.ClientGoal
            };

            HelperMethods.Log($"Sending goal to server");

            session.Socket.SendPacket(statusUpdatePacket);
        }

        public ReadOnlyCollection<long> GetLocationsChecked() => session.Locations.AllLocationsChecked;
        public int AllLocationsCount => session.Locations.AllLocations.Count;

        public string GetPlayerNameFromSlot(int slot)
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
            HelperMethods.Log($"Send location hint for location with id: {id} to server");
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

        internal int GetServerDataStorage(string key)
        {
            return session.DataStorage[key];
        }

        internal void UpdateServerDataStorage(string key, int value)
        {
            HelperMethods.Log($"Update Server Data Storage {key} to {value}");
            session.DataStorage[key] = value;
        }
    }
}
