using System;
using LuminaMatch.Economy;
using UnityEngine;
#if UNITY_ADS
using UnityEngine.Advertisements;
#endif

namespace LuminaMatch.Monetization
{
    /// <summary>
    /// Unity Ads rewarded + interstitial. IDs in docs/STORE_ADS_SETUP.md / AdsConfig.
    /// </summary>
    public class UnityAdsService : IAdsService
#if UNITY_ADS
        , IUnityAdsInitializationListener, IUnityAdsLoadListener, IUnityAdsShowListener
#endif
    {
        public static string GameIdIos = "PLACEHOLDER_IOS_GAME_ID";
        public static string GameIdAndroid = "PLACEHOLDER_ANDROID_GAME_ID";
        public static string RewardedIos = "Rewarded_iOS";
        public static string RewardedAndroid = "Rewarded_Android";
        public static string InterstitialIos = "Interstitial_iOS";
        public static string InterstitialAndroid = "Interstitial_Android";

        static float _lastInterstitialRealtime;
        Action<bool> _rewardedCallback;
        bool _rewardedReady;

        public bool IsRewardedReady
        {
            get
            {
#if UNITY_ADS
                return _rewardedReady || HasPlaceholderIds();
#else
                return true;
#endif
            }
        }

        public UnityAdsService()
        {
#if UNITY_ADS
            if (HasPlaceholderIds())
            {
                Debug.LogWarning("[LuminaMatch] Unity Ads Game IDs are placeholders — using simulated ads.");
                return;
            }

            string gameId = Application.platform == RuntimePlatform.IPhonePlayer ? GameIdIos : GameIdAndroid;
            Advertisement.Initialize(gameId, testMode: Debug.isDebugBuild, this);
#else
            Debug.LogWarning("[LuminaMatch] com.unity.ads not resolved — simulating ads.");
#endif
        }

        static bool HasPlaceholderIds()
            => GameIdIos.StartsWith("PLACEHOLDER") || GameIdAndroid.StartsWith("PLACEHOLDER");

        public void ShowRewarded(Action<bool> onCompleted)
        {
#if UNITY_ADS
            if (HasPlaceholderIds() || !_rewardedReady)
            {
                Debug.LogWarning("[LuminaMatch] Rewarded not ready — simulating success.");
                onCompleted?.Invoke(true);
                return;
            }

            _rewardedCallback = onCompleted;
            Advertisement.Show(RewardedPlacement(), this);
#else
            onCompleted?.Invoke(true);
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
                Debug.Log("[LuminaMatch] Interstitial simulated (placeholder Game IDs).");
                return;
            }
            Advertisement.Show(InterstitialPlacement(), this);
#else
            Debug.Log("[LuminaMatch] Interstitial simulated.");
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
