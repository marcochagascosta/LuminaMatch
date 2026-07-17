using System;
using LuminaMatch.Economy;
using UnityEngine;

namespace LuminaMatch.Monetization
{
    /// <summary>
    /// Production ads stub. Replace with Unity Ads / LevelPlay when Game IDs are set
    /// in docs/STORE_ADS_SETUP.md.
    /// </summary>
    public class UnityAdsService : IAdsService
    {
        static float _lastInterstitialUtc;

        public bool IsRewardedReady => true;

        public void ShowRewarded(Action<bool> onCompleted)
        {
            Debug.LogWarning("[LuminaMatch] Unity Ads not configured — simulating rewarded completion.");
            onCompleted?.Invoke(true);
        }

        public void ShowInterstitialIfAllowed()
        {
            var progress = PlayerProgress.Instance;
            if (progress != null && progress.Data.TutorialStep < 3)
                return;

            if (progress != null && progress.Data.RemoveAds)
                return;

            var now = Time.realtimeSinceStartup;
            if (now - _lastInterstitialUtc < 180f)
                return;

            _lastInterstitialUtc = now;
            Debug.LogWarning("[LuminaMatch] Unity Ads not configured — interstitial skipped (frequency cap applied).");
        }
    }
}
