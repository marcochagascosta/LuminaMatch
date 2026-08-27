namespace LuminaMatch.Economy
{
    /// <summary>In-game coin sinks (not real-money IAP).</summary>
    public static class SoftShop
    {
        // Tuned so early coin rewards (~50–100) make boosters reachable without farming.
        public const int LifeCost = 350;
        public const int LivesFullCost = 1200;
        public const int HammerCost = 200;
        public const int SwapCost = 200;
        public const int LineBlastCost = 250;

        public static bool TryBuyLife(PlayerProgress p)
        {
            if (p == null || p.Data.Lives >= p.Data.MaxLives) return false;
            if (!p.TrySpendCoins(LifeCost)) return false;
            p.AddLives(1);
            return true;
        }

        public static bool TryBuyFullLives(PlayerProgress p)
        {
            if (p == null || p.Data.Lives >= p.Data.MaxLives) return false;
            if (!p.TrySpendCoins(LivesFullCost)) return false;
            p.Data.Lives = p.Data.MaxLives;
            p.Data.NextLifeUtcTicks = 0;
            p.Save();
            return true;
        }

        public static bool TryBuyBooster(PlayerProgress p, BoosterType type)
        {
            if (p == null) return false;
            int cost = type switch
            {
                BoosterType.Hammer => HammerCost,
                BoosterType.Swap => SwapCost,
                BoosterType.LineBlast => LineBlastCost,
                _ => 0
            };
            if (cost <= 0 || !p.TrySpendCoins(cost)) return false;
            p.AddBooster(type, 1);
            return true;
        }
    }
}
