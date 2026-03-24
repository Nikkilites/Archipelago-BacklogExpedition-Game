namespace BEx.Core
{
    public class GameSession
    {
        public ConnectionHandler ConnectionHandler { get; }
        public DataStorageHandler DataStorageHandler { get; }
        public ItemHandler ItemHandler { get; }
        public RegionHandler RegionHandler { get; }
        public GoalHandler GoalHandler { get; }
        public HintHandler HintHandler { get; }
        public ITextClient TextClient { get; }


        public GameSession(ILogger logger, ITextClient textClient, IMessageService messages, IDataLoader loader)
        {
            ConnectionHandler = new ConnectionHandler(this, logger, textClient);
            DataStorageHandler = new DataStorageHandler(logger, loader);
            ItemHandler = new ItemHandler(this, logger);
            RegionHandler = new RegionHandler(this, logger);
            GoalHandler = new GoalHandler(this, logger);
            HintHandler = new HintHandler(this, logger);
            TextClient = textClient;
        }
    }
}
