using Archipelago.MultiClient.Net.Helpers;

namespace Backlog_Expedition
{
    public class ItemHandler
    {
        private static readonly object itemLock = new();
        private readonly Queue<string> itemQueue = new();
        private List<string> _runes = [];
        public List<string> AvailableRunes { 
            get
            {
                return _runes;
            }
            private set
            {
                _runes = value;
            }
        }
        private int trashAquired = 0;
        private int trashUsed 
        { 
            get
            {
                return GameHandler.ConnectionHandler.GetServerDataStorage("trash_used");
            }
            set
            {
                GameHandler.ConnectionHandler.UpdateServerDataStorage("trash_used", value);
            }
        }
        public int TrashAvailable => trashAquired - trashUsed;
        public int TrashInWorld => GameHandler.ConnectionHandler.AllLocationsCount - GameHandler.RegionHandler.Regions.Count;

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
                AvailableRunes.Add(item);
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
    }
}
