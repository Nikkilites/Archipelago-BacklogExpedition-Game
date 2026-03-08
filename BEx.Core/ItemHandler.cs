using Archipelago.MultiClient.Net.Helpers;
using System.Collections.Concurrent;

namespace BEx.Core
{
    public class ItemHandler
    {
        private readonly GameSession _session;
        private readonly ILogger _logger;

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
                return _session.ConnectionHandler.GetServerDataStorage(trashServerDataStorageKey);
            }
            set
            {
                _session.ConnectionHandler.UpdateServerDataStorage(trashServerDataStorageKey, value);
            }
        }

        public int TrashAvailable => _trashAcquired - trashUsed;
        public int TrashInWorld => _session.ConnectionHandler.AllLocationsCount - _session.RegionHandler.Regions.Count;

        private string trashServerDataStorageKey = "";

        public ItemHandler(GameSession session, ILogger logger)
        {
            _session = session;
            _logger = logger;

            foreach (var region in _session.DataStorageHandler.Regions)
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
            _logger.Log($"{_session.ConnectionHandler.PlayerName} received item: {item}");
        }

        public void UseTrash(int amount)
        {
            lock (_lock)
            {
                trashUsed += amount;
                _logger.Log($"{_session.ConnectionHandler.PlayerName} used {amount} trash, remaining {TrashAvailable}");
                OnItemsUpdated?.Invoke(); // notify UI
            }
        }

        public void SetupItemHandler()
        {
            int slotId = _session.ConnectionHandler.GetThisSlotId();
            string slotName = _session.ConnectionHandler.GetPlayerNameFromSlot(slotId);
            string key = $"BEx_slot:{slotId}_{slotName}:trash_used";
            trashServerDataStorageKey = key;
        }
    }
}
