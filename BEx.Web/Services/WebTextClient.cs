using BEx.Core;

namespace BEx.Web.Services
{
    public class WebTextClient : ITextClient
    {
        private readonly object _lock = new();

        private List<string> textLines = new List<string>();
        private const int MAX_TEXT_LINES = 80;

        public event Action? OnMessageAdded;

        public IReadOnlyList<string> GetLines()
        {
            lock (_lock)
            {
                return textLines.ToArray();
            }
        }

        public void ShowMessage(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return;

            lock (_lock)
            {
                if (textLines.Count >= MAX_TEXT_LINES)
                {
                    textLines.RemoveAt(0);
                }

                textLines.Add(message);
            }

            OnMessageAdded?.Invoke();
        }

        public void SendMessageToServer(GameSession session, string message)
        {
            session.ConnectionHandler.SendMessage(message);
        }
    }
}
