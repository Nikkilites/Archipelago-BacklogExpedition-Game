using Archipelago.MultiClient.Net.Helpers;
using System.Collections.Concurrent;

namespace BEx.Core
{
    public class ItemHandler
    {
        private readonly GameSession _gameSession;

        private readonly ConcurrentQueue<string> _itemQueue = new();
        private readonly object _lock = new();
        private CancellationTokenSource _cts;
        public event Action OnItemsUpdated;

        public Dictionary<string, int> AvailableRunes = new();

        private int _trashAcquired = 0;
        private int trashUsed
        {
            get
            {
                return _gameSession.ConnectionHandler.GetServerDataStorage(trashServerDataStorageKey);
            }
            set
            {
                _gameSession.ConnectionHandler.UpdateServerDataStorage(trashServerDataStorageKey, value);
            }
        }

        public int TrashAvailable => _trashAcquired - trashUsed;
        public int TrashInWorld => _gameSession.ConnectionHandler.AllLocationsCount - _gameSession.RegionHandler.Regions.Count;

        private string trashServerDataStorageKey = "";

        public ItemHandler(GameSession session)
        {
            _gameSession = session;

            foreach (var region in _gameSession.DataStorageHandler.Regions)
                AvailableRunes[$"{region} Rune"] = 0;

            _cts = new CancellationTokenSource();
            _ = ProcessQueueAsync(_cts.Token);
        }

        public void Stop() => _cts?.Cancel();

        public void OnItemReceived(IReceivedItemsHelper helper)
        {
            string item = helper.PeekItem().ItemName;
            helper.DequeueItem();

            _itemQueue.Enqueue(item);
        }

        private async Task ProcessQueueAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                bool updated = false;

                while (_itemQueue.TryDequeue(out var item))
                {
                    lock (_lock)
                    {
                        GiveItem(item);
                        updated = true;
                    }
                }

                if (updated)
                {
                    // trigger UI update
                    OnItemsUpdated?.Invoke();
                }

                await Task.Delay(10, token); // small delay
            }
        }

        private void GiveItem(string item)
        {
            if (item.EndsWith("Rune") && !item.StartsWith("Broken"))
            {
                AvailableRunes[item]++;
            }
            else
            {
                _trashAcquired++;
            }
            // _logger.Log($"{_session.ConnectionHandler.PlayerName} received item: {item}");
        }

        public void UseTrash(int amount)
        {
            lock (_lock)
            {
                trashUsed += amount;
                _gameSession.Logger.Log($"{_gameSession.ConnectionHandler.PlayerName} used {amount} trash, remaining {TrashAvailable}");
                OnItemsUpdated?.Invoke(); // notify UI
            }
        }

        public void SetupItemHandler()
        {
            int slotId = _gameSession.ConnectionHandler.GetThisSlotId();
            string slotName = _gameSession.ConnectionHandler.GetPlayerNameFromSlot(slotId);
            string key = $"BEx_slot:{slotId}_{slotName}:trash_used";
            trashServerDataStorageKey = key;
        }
    }
}
