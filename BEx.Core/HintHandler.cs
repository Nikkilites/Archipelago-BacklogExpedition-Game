using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Models;

namespace BEx.Core
{
    public class HintHandler(GameSession gameSession, ILogger logger)
    {
        private readonly GameSession _gameSession = gameSession;
        private readonly ILogger _logger = logger;


        private List<Hint>? _allHints;
        private DateTime _lastFetch = DateTime.MinValue;
        private readonly TimeSpan _cacheDuration = TimeSpan.FromSeconds(30);

        public List<Hint> AllHints
        {
            get
            {
                if (_allHints == null || DateTime.UtcNow - _lastFetch > _cacheDuration)
                {
                    _allHints = _gameSession.ConnectionHandler.GetHints().ToList();
                    _lastFetch = DateTime.UtcNow;
                }

                return _allHints;
            }
        }

        public string GetHintColor(ItemFlags flags)
        {
            switch (flags)
            {
                case ItemFlags.Advancement:
                    return "#AF99EF";
                case ItemFlags.NeverExclude:
                    return "#6D8BE8";
                case ItemFlags.Trap:
                    return "#FA8072";
                default:
                    return "#00EEEE";
            }
        }

        public int GetHintCost()
        {
            int hintCost = (int)Math.Round(_gameSession.ItemHandler.TrashInWorld / 20.0, MidpointRounding.AwayFromZero);
            if (hintCost <= 1)
                hintCost = 1;

            return hintCost;
        }
    }
}
