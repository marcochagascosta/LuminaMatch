using UnityEngine;

namespace LuminaMatch.Monetization
{
    /// <summary>Loads AdMob IDs from Resources/Monetization/AdsConfig.json.</summary>
    [System.Serializable]
    public class AdsConfigData
    {
        public string androidAppId = "ca-app-pub-3940256099942544~3347511713";
        public string iosAppId = "ca-app-pub-3940256099942544~1458002511";
        public string rewardedAndroid = "ca-app-pub-3940256099942544/5224354917";
        public string rewardedIos = "ca-app-pub-3940256099942544/1712485313";
        public string interstitialAndroid = "ca-app-pub-3940256099942544/1033173712";
        public string interstitialIos = "ca-app-pub-3940256099942544/4411468910";
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
