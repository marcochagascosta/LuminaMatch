using System;
using LuminaMatch.Economy;
using UnityEngine;
#if UNITY_ADS
using UnityEngine.Advertisements;
#endif

namespace LuminaMatch.Monetization
{
    /// <summary>
    /// Unity Ads rewarded + interstitial. IDs in docs/STORE_ADS_SETUP.md
    /// Release builds with placeholder IDs do NOT grant free rewards.
    /// </summary>
    public class UnityAdsService : IAdsService
#if UNITY_ADS
        , IUnityAdsInitializationListener, IUnityAdsLoadListener, IUnityAdsShowListener
#endif
    {
        public static string RewardedIos => AdsConfig.Current.rewardedIos;
        public static string RewardedAndroid => AdsConfig.Current.rewardedAndroid;
        public static string InterstitialIos => AdsConfig.Current.interstitialIos;
        public static string InterstitialAndroid => AdsConfig.Current.interstitialAndroid;

        static float _lastInterstitialRealtime;
        Action<bool> _rewardedCallback;
        bool _rewardedReady;

        static bool SimulateAds
            => HasPlaceholderIds() && (Application.isEditor || Debug.isDebugBuild);

        public bool IsRewardedReady
        {
            get
            {
#if UNITY_ADS
                if (SimulateAds) return true;
                return _rewardedReady;
#else
                return SimulateAds;
#endif
            }
        }

        public UnityAdsService()
        {
#if UNITY_ADS
            if (HasPlaceholderIds())
            {
                Debug.LogWarning(SimulateAds
                    ? "[LuminaMatch] Unity Ads placeholders — simulating ads (dev)."
                    : "[LuminaMatch] Unity Ads placeholders — rewarded blocked in release until Game IDs are set.");
                return;
            }

            string gameId = Application.platform == RuntimePlatform.IPhonePlayer
                ? AdsConfig.Current.iosAppId
                : AdsConfig.Current.androidAppId;
            Advertisement.Initialize(gameId, testMode: Debug.isDebugBuild, this);
#else
            Debug.LogWarning("[LuminaMatch] com.unity.ads not resolved.");
#endif
        }

        static bool HasPlaceholderIds() => AdsConfig.HasPlaceholderIds();

        public void ShowRewarded(Action<bool> onCompleted)
        {
#if UNITY_ADS
            if (SimulateAds)
            {
                Debug.LogWarning("[LuminaMatch] Rewarded simulated (dev).");
                onCompleted?.Invoke(true);
                return;
            }

            if (HasPlaceholderIds() || !_rewardedReady)
            {
                Debug.LogWarning("[LuminaMatch] Rewarded not ready — no free grant in release.");
                onCompleted?.Invoke(false);
                return;
            }

            _rewardedCallback = onCompleted;
            Advertisement.Show(RewardedPlacement(), this);
#else
            if (SimulateAds)
                onCompleted?.Invoke(true);
            else
                onCompleted?.Invoke(false);
#endif
        }

        public void ShowInterstitialIfAllowed()
        {
            var progress = PlayerProgress.Instance;
            if (progress != null && (progress.Data.TutorialStep < 3 || progress.Data.RemoveAds))
                return;

            if (Time.realtimeSinceStartup - _lastInterstitialRealtime < 180f)
                return;

            _lastInterstitialRealtime = Time.realtimeSinceStartup;

#if UNITY_ADS
            if (HasPlaceholderIds())
            {
                Debug.Log("[LuminaMatch] Interstitial skipped (placeholder Game IDs).");
                return;
            }
            Advertisement.Show(InterstitialPlacement(), this);
#else
            Debug.Log("[LuminaMatch] Interstitial skipped (no ads package).");
#endif
        }

        static string RewardedPlacement()
            => Application.platform == RuntimePlatform.IPhonePlayer ? RewardedIos : RewardedAndroid;

        static string InterstitialPlacement()
            => Application.platform == RuntimePlatform.IPhonePlayer ? InterstitialIos : InterstitialAndroid;

#if UNITY_ADS
        public void OnInitializationComplete()
        {
            Debug.Log("[LuminaMatch] Unity Ads initialized.");
            Advertisement.Load(RewardedPlacement(), this);
            Advertisement.Load(InterstitialPlacement(), this);
        }

        public void OnInitializationFailed(UnityAdsInitializationError error, string message)
            => Debug.LogError($"[LuminaMatch] Ads init failed: {error} {message}");

        public void OnUnityAdsAdLoaded(string placementId)
        {
            if (placementId == RewardedPlacement())
                _rewardedReady = true;
        }

        public void OnUnityAdsFailedToLoad(string placementId, UnityAdsLoadError error, string message)
            => Debug.LogWarning($"[LuminaMatch] Ads load failed {placementId}: {error}");

        public void OnUnityAdsShowFailure(string placementId, UnityAdsShowError error, string message)
        {
            if (placementId == RewardedPlacement())
            {
                _rewardedCallback?.Invoke(false);
                _rewardedCallback = null;
            }
        }

        public void OnUnityAdsShowStart(string placementId) { }

        public void OnUnityAdsShowClick(string placementId) { }

        public void OnUnityAdsShowComplete(string placementId, UnityAdsShowCompletionState showCompletionState)
        {
            if (placementId == RewardedPlacement())
            {
                bool ok = showCompletionState == UnityAdsShowCompletionState.COMPLETED;
                _rewardedCallback?.Invoke(ok);
                _rewardedCallback = null;
                Advertisement.Load(RewardedPlacement(), this);
            }
            else
            {
                Advertisement.Load(InterstitialPlacement(), this);
            }
        }
#endif
    }
}
