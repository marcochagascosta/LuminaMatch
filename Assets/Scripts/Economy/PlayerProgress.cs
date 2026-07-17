using System;
using UnityEngine;

namespace LuminaMatch.Economy
{
    [Serializable]
    public class PlayerSaveData
    {
        public int Coins = 200;
        public int Lives = 5;
        public int MaxLives = 5;
        public long NextLifeUtcTicks;
        public int HighestUnlockedLevel = 1;
        public int CastlePieces = 0;
        public int Hammers = 3;
        public int Swaps = 3;
        public int LineBlasts = 2;
        public bool RemoveAds;
        public int LevelsWon;
        public int TutorialStep;
        public bool StarterPackBought;
        public string DailyOfferDayKey;
        public bool StarterPackSeen;
    }

    public class PlayerProgress
    {
        const string Key = "LuminaMatch.Save.v1";
        public const int LifeRegenSeconds = 20 * 60;
        public const int ContinueCost = 900;
        public const int LevelsPerCastlePiece = 3;

        public PlayerSaveData Data { get; private set; }

        public static PlayerProgress Instance { get; private set; }

        public PlayerProgress()
        {
            Instance = this;
            Load();
            TickLives();
        }

        public void Load()
        {
            if (PlayerPrefs.HasKey(Key))
            {
                try
                {
                    Data = JsonUtility.FromJson<PlayerSaveData>(PlayerPrefs.GetString(Key));
                    if (Data == null) Data = new PlayerSaveData();
                }
                catch
                {
                    Data = new PlayerSaveData();
                }
            }
            else
            {
                Data = new PlayerSaveData();
                Save();
            }
        }

        public void Save()
        {
            PlayerPrefs.SetString(Key, JsonUtility.ToJson(Data));
            PlayerPrefs.Save();
        }

        public void TickLives()
        {
            if (Data.Lives >= Data.MaxLives)
            {
                Data.NextLifeUtcTicks = 0;
                return;
            }

            if (Data.NextLifeUtcTicks <= 0)
            {
                Data.NextLifeUtcTicks = DateTime.UtcNow.AddSeconds(LifeRegenSeconds).Ticks;
                Save();
                return;
            }

            var next = new DateTime(Data.NextLifeUtcTicks, DateTimeKind.Utc);
            while (Data.Lives < Data.MaxLives && DateTime.UtcNow >= next)
            {
                Data.Lives++;
                next = next.AddSeconds(LifeRegenSeconds);
            }

            Data.NextLifeUtcTicks = Data.Lives >= Data.MaxLives ? 0 : next.Ticks;
            Save();
        }

        public int SecondsToNextLife()
        {
            TickLives();
            if (Data.Lives >= Data.MaxLives || Data.NextLifeUtcTicks <= 0) return 0;
            var next = new DateTime(Data.NextLifeUtcTicks, DateTimeKind.Utc);
            return Math.Max(0, (int)(next - DateTime.UtcNow).TotalSeconds);
        }

        public bool TrySpendLife()
        {
            TickLives();
            if (Data.Lives <= 0) return false;
            Data.Lives--;
            if (Data.Lives < Data.MaxLives && Data.NextLifeUtcTicks <= 0)
                Data.NextLifeUtcTicks = DateTime.UtcNow.AddSeconds(LifeRegenSeconds).Ticks;
            Save();
            return true;
        }

        public void AddLives(int n)
        {
            Data.Lives = Math.Min(Data.MaxLives + 5, Data.Lives + n);
            if (Data.Lives >= Data.MaxLives) Data.NextLifeUtcTicks = 0;
            Save();
        }

        public bool TrySpendCoins(int amount)
        {
            if (Data.Coins < amount) return false;
            Data.Coins -= amount;
            Save();
            return true;
        }

        public void AddCoins(int amount)
        {
            Data.Coins += amount;
            Save();
        }

        public void OnLevelWon(int levelId, int coinReward)
        {
            Data.LevelsWon++;
            AddCoins(coinReward);
            if (levelId >= Data.HighestUnlockedLevel)
                Data.HighestUnlockedLevel = Math.Min(LevelCatalogRef.Total, levelId + 1);

            if (Data.LevelsWon % LevelsPerCastlePiece == 0)
                Data.CastlePieces++;
            Save();
        }

        public bool TryUseBooster(BoosterType type)
        {
            switch (type)
            {
                case BoosterType.Hammer when Data.Hammers > 0:
                    Data.Hammers--; Save(); return true;
                case BoosterType.Swap when Data.Swaps > 0:
                    Data.Swaps--; Save(); return true;
                case BoosterType.LineBlast when Data.LineBlasts > 0:
                    Data.LineBlasts--; Save(); return true;
                default:
                    return false;
            }
        }

        public void AddBooster(BoosterType type, int amount)
        {
            switch (type)
            {
                case BoosterType.Hammer: Data.Hammers += amount; break;
                case BoosterType.Swap: Data.Swaps += amount; break;
                case BoosterType.LineBlast: Data.LineBlasts += amount; break;
            }
            Save();
        }

        public void GrantRemoveAds()
        {
            Data.RemoveAds = true;
            Save();
        }

        public void ResetForDebug()
        {
            Data = new PlayerSaveData();
            Save();
        }
    }

    public enum BoosterType
    {
        Hammer,
        Swap,
        LineBlast
    }

    /// <summary>Avoid circular asm refs — mirrors LevelCatalog.TotalLevels.</summary>
    static class LevelCatalogRef
    {
        public const int Total = 60;
    }
}
