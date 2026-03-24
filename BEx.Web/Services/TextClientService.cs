using BEx.Core;

namespace BEx.Web.Services
{
    public class TextClientService : ITextClient
    {
        private static List<string> textLines = new List<string>();
        private const int MAX_TEXT_LINES = 80;

        public event Action? OnMessageAdded;

        public IReadOnlyList<string> GetLines()
        {
            return textLines;
        }

        public void ShowMessage(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return;

            if (textLines.Count == MAX_TEXT_LINES)
            {
                textLines.RemoveAt(0);
            }

            textLines.Add(message);

            OnMessageAdded?.Invoke();
        }

        public void SendMessageToServer(GameSession session, string message)
        {
            session.ConnectionHandler.SendMessage(message);
        }
    }
}
