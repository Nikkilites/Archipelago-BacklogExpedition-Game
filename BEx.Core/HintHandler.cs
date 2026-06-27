using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Models;

namespace BEx.Core
{
    public class HintHandler(GameSession gameSession)
    {
        private readonly GameSession _gameSession = gameSession;

        public List<Hint> AllHints { get; private set; } = new();
        public event Action? OnHintsUpdated;

        public double HintCostPercentage { get; set; } = 20;

        private DateTime _lastFetch = DateTime.MinValue;
        private readonly TimeSpan _cacheDuration = TimeSpan.FromSeconds(10);

        private readonly SemaphoreSlim _updateLock = new(1, 1);

        public async Task UpdateHints()
        {
            await _updateLock.WaitAsync();

            try
            {
                if (DateTime.UtcNow - _lastFetch <= _cacheDuration)
                    return;

                AllHints = await GetAllHintsAsync();
                OnHintsUpdated?.Invoke();
            }
            catch (Exception ex)
            {
                _gameSession.Logger.Log($"Hint refresh failed: {ex}");
            }
            finally
            {
                _updateLock.Release();
            }
        }

        public async Task<List<Hint>> GetAllHintsAsync()
        {
            if (DateTime.UtcNow - _lastFetch > _cacheDuration)
            {
                AllHints = (await _gameSession.ConnectionHandler.GetHintsAsync()).ToList();
                _lastFetch = DateTime.UtcNow;
            }

            return AllHints;
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
            int hintCost = (int)Math.Round(_gameSession.ItemHandler.TrashInWorld * (HintCostPercentage/100), MidpointRounding.AwayFromZero);
            if (hintCost <= 1)
                hintCost = 1;

            return hintCost;
        }
    }
}
