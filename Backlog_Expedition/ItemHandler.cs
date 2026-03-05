using Archipelago.MultiClient.Net.Helpers;

namespace Backlog_Expedition
{
    public class ItemHandler
    {
        private static readonly object itemLock = new();
        private readonly Queue<string> itemQueue = new();
        public Dictionary<string, int> AvailableRunes = new();
        private int trashAquired = 0;
        private int trashUsed 
        { 
            get
            {
                return GameHandler.ConnectionHandler.GetServerDataStorage(trashServerDataStorageKey);
            }
            set
            {
                GameHandler.ConnectionHandler.UpdateServerDataStorage(trashServerDataStorageKey, value);
            }
        }
        public int TrashAvailable => trashAquired - trashUsed;
        public int TrashInWorld => GameHandler.ConnectionHandler.AllLocationsCount - GameHandler.RegionHandler.Regions.Count;

        private string trashServerDataStorageKey = "";

        public ItemHandler()
        {
            foreach (string region in GameHandler.DataStorageHandler.Regions)
            {
                AvailableRunes.Add($"{region} Rune", 0);
            }
        }

        public void OnItemReceived(IReceivedItemsHelper helper)
        {
            HelperMethods.Log($"Received item from server");

            lock (itemLock)
            {
                string itemName = helper.PeekItem().ItemName;

                itemQueue.Enqueue(itemName);
                HelperMethods.Log($"Enqueued item with name: {itemName}");

                helper.DequeueItem();
            }
        }

        public void SetupFrameUpdater()
        {
            Updater.OnFrameUpdated += OnFrameUpdate;
            Updater.Start();
        }

        public void OnFrameUpdate()
        {
            lock (itemLock)
            {
                if (itemQueue.Count > 0)
                    GiveItem(itemQueue.Dequeue());
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
                trashAquired++;
            }
            HelperMethods.Log($"Processed item with name: {item}");
        }

        public void UseTrash(int amount)
        {
            HelperMethods.Log($"Bought hint with {amount} trash items");
            trashUsed = trashUsed + amount;
        }

        public void SetupItemHandler()
        {
            int slotId = GameHandler.ConnectionHandler.GetThisSlotId();
            string slotName = GameHandler.ConnectionHandler.GetPlayerNameFromSlot(slotId);
            string key = $"BEx_slot:{slotId}_{slotName}:trash_used";
            HelperMethods.Log($"TrashServerDataStorageKey is: {key}");
            trashServerDataStorageKey = key;
        }
    }
}
