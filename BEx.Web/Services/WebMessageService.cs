using BEx.Core;
    
public class WebMessageService : IMessageService
{
    public event Action<string>? OnMessage;

    public void Send(string message)
    {
        OnMessage?.Invoke(message);
    }

    public void ShowMessage(string message, MessageType type = MessageType.Info)
    {
    }
}