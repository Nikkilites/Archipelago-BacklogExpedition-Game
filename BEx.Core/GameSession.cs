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
        public ILogger Logger { get; }


        public GameSession(ILogger logger, ITextClient textClient, IMessageService messages, DataStorageHandler dataStorageHandler)
        {
            Logger = logger;
            DataStorageHandler = dataStorageHandler;
            ConnectionHandler = new ConnectionHandler(this, textClient);
            ItemHandler = new ItemHandler(this);
            RegionHandler = new RegionHandler(this);
            GoalHandler = new GoalHandler(this);
            HintHandler = new HintHandler(this);
            TextClient = textClient;
        }
    }
}
