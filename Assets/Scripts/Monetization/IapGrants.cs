using LuminaMatch.Economy;

namespace LuminaMatch.Monetization
{
    public static class IapGrants
    {
        public static void Apply(IapProductId id, PlayerProgress progress)
        {
            switch (id)
            {
                case IapProductId.CoinsSmall:
                    progress.AddCoins(500);
                    break;
                case IapProductId.CoinsMedium:
                    progress.AddCoins(1500);
                    break;
                case IapProductId.CoinsLarge:
                    progress.AddCoins(5000);
                    break;
                case IapProductId.LivesRefill:
                    RefillLives(progress);
                    break;
                case IapProductId.BoosterPack:
                    progress.AddBooster(BoosterType.Hammer, 3);
                    progress.AddBooster(BoosterType.Swap, 3);
                    progress.AddBooster(BoosterType.LineBlast, 3);
                    break;
                case IapProductId.RemoveAds:
                    progress.GrantRemoveAds();
                    break;
                case IapProductId.StarterPack:
                    progress.AddCoins(2000);
                    progress.AddBooster(BoosterType.Hammer, 5);
                    progress.AddBooster(BoosterType.Swap, 5);
                    progress.AddBooster(BoosterType.LineBlast, 5);
                    RefillLives(progress);
                    OfferService.MarkStarterBought(progress);
                    break;
            }
        }

        static void RefillLives(PlayerProgress progress)
        {
            progress.Data.Lives = progress.Data.MaxLives;
            progress.Data.NextLifeUtcTicks = 0;
            progress.Save();
        }
    }
}
