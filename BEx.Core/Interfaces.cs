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

    public interface IDataLoader
    {
        T Load<T>(string path);
        StoryData LoadStory();
        GameData LoadData();
    }
}
