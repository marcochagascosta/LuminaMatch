using System;

namespace LuminaMatch.Economy
{
    public static class OfferService
    {
        public static bool ShouldShowStarterPack(PlayerSaveData d)
            => !d.StarterPackBought && d.LevelsWon <= 5 && d.TutorialStep >= 3;

        public static bool ShouldShowDailyOffer(PlayerSaveData d, DateTime utcNow)
        {
            if (!d.StarterPackBought && d.TutorialStep < 3)
                return false;

            var today = utcNow.ToString("yyyy-MM-dd");
            return d.DailyOfferDayKey != today;
        }

        public static void MarkDailyClaimed(PlayerSaveData d, DateTime utcNow)
        {
            d.DailyOfferDayKey = utcNow.ToString("yyyy-MM-dd");
        }

        public static void MarkStarterBought(PlayerSaveData d)
        {
            d.StarterPackBought = true;
        }

        public static void MarkDailyClaimed(PlayerProgress p, DateTime utcNow)
        {
            MarkDailyClaimed(p.Data, utcNow);
            p.Save();
        }

        public static void MarkStarterBought(PlayerProgress p)
        {
            MarkStarterBought(p.Data);
            p.Save();
        }
    }
}
