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
            if (item.EndsWith("Rune"))
            {
                AvailableRunes.Add(item);
                HelperMethods.Log($"Processed item with name: {item}");
            }
        }
    }
}
