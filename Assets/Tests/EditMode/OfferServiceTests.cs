using System;
using LuminaMatch.Economy;
using NUnit.Framework;

namespace LuminaMatch.Tests
{
    public class OfferServiceTests
    {
        [Test]
        public void ShouldShowStarterPack_WhenEligible_ReturnsTrue()
        {
            var d = new PlayerSaveData
            {
                StarterPackBought = false,
                LevelsWon = 3,
                TutorialStep = 3
            };
            Assert.IsTrue(OfferService.ShouldShowStarterPack(d));
        }

        [Test]
        public void ShouldShowStarterPack_WhenAlreadyBought_ReturnsFalse()
        {
            var d = new PlayerSaveData
            {
                StarterPackBought = true,
                LevelsWon = 2,
                TutorialStep = 3
            };
            Assert.IsFalse(OfferService.ShouldShowStarterPack(d));
        }

        [Test]
        public void ShouldShowStarterPack_WhenTooManyWins_ReturnsFalse()
        {
            var d = new PlayerSaveData
            {
                StarterPackBought = false,
                LevelsWon = 6,
                TutorialStep = 3
            };
            Assert.IsFalse(OfferService.ShouldShowStarterPack(d));
        }

        [Test]
        public void ShouldShowStarterPack_BeforeTutorialDone_ReturnsFalse()
        {
            var d = new PlayerSaveData
            {
                StarterPackBought = false,
                LevelsWon = 1,
                TutorialStep = 2
            };
            Assert.IsFalse(OfferService.ShouldShowStarterPack(d));
        }

        [Test]
        public void ShouldShowDailyOffer_NotClaimedToday_ReturnsTrue()
        {
            var d = new PlayerSaveData { TutorialStep = 3, DailyOfferDayKey = "2026-07-15" };
            var now = new DateTime(2026, 7, 16, 12, 0, 0, DateTimeKind.Utc);
            Assert.IsTrue(OfferService.ShouldShowDailyOffer(d, now));
        }

        [Test]
        public void ShouldShowDailyOffer_AlreadyClaimedToday_ReturnsFalse()
        {
            var d = new PlayerSaveData { TutorialStep = 3, DailyOfferDayKey = "2026-07-16" };
            var now = new DateTime(2026, 7, 16, 18, 0, 0, DateTimeKind.Utc);
            Assert.IsFalse(OfferService.ShouldShowDailyOffer(d, now));
        }

        [Test]
        public void ShouldShowDailyOffer_BeforeTutorialWithoutStarter_ReturnsFalse()
        {
            var d = new PlayerSaveData
            {
                StarterPackBought = false,
                TutorialStep = 1,
                DailyOfferDayKey = null
            };
            var now = new DateTime(2026, 7, 16, 12, 0, 0, DateTimeKind.Utc);
            Assert.IsFalse(OfferService.ShouldShowDailyOffer(d, now));
        }

        [Test]
        public void ShouldShowDailyOffer_AfterStarterBought_ReturnsTrue()
        {
            var d = new PlayerSaveData
            {
                StarterPackBought = true,
                TutorialStep = 1,
                DailyOfferDayKey = null
            };
            var now = new DateTime(2026, 7, 16, 12, 0, 0, DateTimeKind.Utc);
            Assert.IsTrue(OfferService.ShouldShowDailyOffer(d, now));
        }

        [Test]
        public void MarkDailyClaimed_SetsDayKey()
        {
            var d = new PlayerSaveData();
            var now = new DateTime(2026, 7, 16, 8, 30, 0, DateTimeKind.Utc);
            OfferService.MarkDailyClaimed(d, now);
            Assert.AreEqual("2026-07-16", d.DailyOfferDayKey);
            Assert.IsFalse(OfferService.ShouldShowDailyOffer(d, now));
        }

        [Test]
        public void MarkStarterBought_HidesStarterPack()
        {
            var d = new PlayerSaveData { LevelsWon = 2, TutorialStep = 3 };
            OfferService.MarkStarterBought(d);
            Assert.IsTrue(d.StarterPackBought);
            Assert.IsFalse(OfferService.ShouldShowStarterPack(d));
        }
    }
}
