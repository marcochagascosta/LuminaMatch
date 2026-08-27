using UnityEngine;

namespace LuminaMatch.Monetization
{
    /// <summary>Loads AdMob IDs from Resources/Monetization/AdsConfig.json.</summary>
    [System.Serializable]
    public class AdsConfigData
    {
        // Empty defaults: missing JSON must not silently ship Google sample ads.
        // Production IDs live in Resources/Monetization/AdsConfig.json.
        public string androidAppId = "";
        public string iosAppId = "";
        public string rewardedAndroid = "";
        public string rewardedIos = "";
        public string interstitialAndroid = "";
        public string interstitialIos = "";
    }

    public static class AdsConfig
    {
        const string ResourcePath = "Monetization/AdsConfig";
        const string GoogleSamplePublisher = "3940256099942544";
        static AdsConfigData _cached;

        public static AdsConfigData Current
        {
            get
            {
                if (_cached != null) return _cached;
                var asset = Resources.Load<TextAsset>(ResourcePath);
                if (asset != null && !string.IsNullOrWhiteSpace(asset.text))
                {
                    try
                    {
                        _cached = JsonUtility.FromJson<AdsConfigData>(asset.text);
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogWarning($"[LuminaMatch] AdsConfig JSON invalid: {ex.Message}");
                    }
                }
                _cached ??= new AdsConfigData();
                return _cached;
            }
        }

        public static bool HasPlaceholderIds()
        {
            var c = Current;
            return string.IsNullOrEmpty(c.androidAppId) || c.androidAppId.StartsWith("PLACEHOLDER")
                || string.IsNullOrEmpty(c.iosAppId) || c.iosAppId.StartsWith("PLACEHOLDER");
        }

        public static bool IsGoogleSampleIds()
        {
            var c = Current;
            return (c.androidAppId ?? "").Contains(GoogleSamplePublisher)
                || (c.rewardedAndroid ?? "").Contains(GoogleSamplePublisher);
        }

        public static string AppId
        {
            get
            {
#if UNITY_IOS
                return Current.iosAppId;
#else
                return Current.androidAppId;
#endif
            }
        }

        public static string RewardedUnitId
        {
            get
            {
#if UNITY_IOS
                return Current.rewardedIos;
#else
                return Current.rewardedAndroid;
#endif
            }
        }

        public static string InterstitialUnitId
        {
            get
            {
#if UNITY_IOS
                return Current.interstitialIos;
#else
                return Current.interstitialAndroid;
#endif
            }
        }
    }
}
