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
        /// <summary>Once true, Music/Sfx/Vibrate flags are trusted (migration for old saves).</summary>
        public bool SettingsInitialized;
        public bool MusicOn = true;
        public bool SfxOn = true;
        public bool VibrateOn = true;
    }

    public class PlayerProgress
    {
        const string Key = "LuminaMatch.Save.v1";
        public const int LifeRegenSeconds = 30 * 60;
        public const int ContinueCost = 750;
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

            EnsureSettingsDefaults();
        }

        void EnsureSettingsDefaults()
        {
            // v26: force music back on — soft pad in 0.1.25 + toggles left some saves silent.
            const string RepairKey = "LuminaMatch.SettingsRepair.v26";
            if (!PlayerPrefs.HasKey(RepairKey))
            {
                Data.MusicOn = true;
                if (!Data.SettingsInitialized)
                {
                    Data.SfxOn = true;
                    Data.VibrateOn = true;
                }
                Data.SettingsInitialized = true;
                Save();
                PlayerPrefs.SetInt(RepairKey, 1);
                PlayerPrefs.Save();
                return;
            }

            if (Data.SettingsInitialized) return;
            Data.MusicOn = true;
            Data.SfxOn = true;
            Data.VibrateOn = true;
            Data.SettingsInitialized = true;
            Save();
        }

        public void SetMusicOn(bool on)
        {
            Data.MusicOn = on;
            Data.SettingsInitialized = true;
            Save();
            Audio.MusicPlayer.Instance?.ApplyFromSave();
        }

        public void SetSfxOn(bool on)
        {
            Data.SfxOn = on;
            Data.SettingsInitialized = true;
            Save();
        }

        public void SetVibrateOn(bool on)
        {
            Data.VibrateOn = on;
            Data.SettingsInitialized = true;
            Save();
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
                if (Data.NextLifeUtcTicks != 0)
                {
                    Data.NextLifeUtcTicks = 0;
                    Save();
                }
                return;
            }

            if (Data.NextLifeUtcTicks <= 0)
            {
                Data.NextLifeUtcTicks = DateTime.UtcNow.AddSeconds(LifeRegenSeconds).Ticks;
                Save();
                return;
            }

            var next = new DateTime(Data.NextLifeUtcTicks, DateTimeKind.Utc);
            bool gained = false;
            while (Data.Lives < Data.MaxLives && DateTime.UtcNow >= next)
            {
                Data.Lives++;
                next = next.AddSeconds(LifeRegenSeconds);
                gained = true;
            }

            long newTicks = Data.Lives >= Data.MaxLives ? 0 : next.Ticks;
            if (gained || Data.NextLifeUtcTicks != newTicks)
            {
                Data.NextLifeUtcTicks = newTicks;
                Save();
            }
        }

        public int SecondsToNextLife()
        {
            TickLives();
            if (Data.Lives >= Data.MaxLives || Data.NextLifeUtcTicks <= 0) return 0;
            var next = new DateTime(Data.NextLifeUtcTicks, DateTimeKind.Utc);
            return Math.Max(0, (int)(next - DateTime.UtcNow).TotalSeconds);
        }

        public bool HasLife()
        {
            TickLives();
            return Data.Lives > 0;
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
