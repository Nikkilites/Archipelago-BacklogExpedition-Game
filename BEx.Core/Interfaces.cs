using BEx.Core.Model;

namespace BEx.Core
{
    public interface ILogger
    {
        void Log(string message);
    }

    public interface IMessageService
    {
        void ShowMessage(string message, MessageType type = MessageType.Info);
    }

    public enum MessageType
    {
        Info,
        Warning,
        Error
    }

    public interface ITextClient
    {
        public event Action? OnMessageAdded;
        IReadOnlyList<string> GetLines();
        void ShowMessage(string message);
        void SendMessageToServer(GameSession session, string message);
    }

    public interface IDataLoader
    {
        T Load<T>(string path);
        StoryData LoadStory();
        GameData LoadData();
    }
}
