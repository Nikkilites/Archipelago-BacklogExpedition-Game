namespace BEx.Core
{
    public class GameSession
    {
        public ConnectionHandler ConnectionHandler { get; }
        public DataStorageHandler DataStorageHandler { get; }
        public ItemHandler ItemHandler { get; }
        public RegionHandler RegionHandler { get; }
        public GoalHandler GoalHandler { get; }


        public GameSession(ILogger logger, IMessageService messages, IDataLoader loader)
        {
            ConnectionHandler = new ConnectionHandler(this, logger);
            DataStorageHandler = new DataStorageHandler(logger, loader);
            ItemHandler = new ItemHandler(this, logger);
            RegionHandler = new RegionHandler(this, logger);
            GoalHandler = new GoalHandler(this, logger);
        }

        //public event Func<Task>? SessionEnded;

        //public async Task EndSession()
        //{
        //    if (SessionEnded != null)
        //    {
        //        foreach (var handler in SessionEnded.GetInvocationList().Cast<Func<Task>>())
        //        {
        //            await handler();
        //        }
        //    }
        //}
    }
}
