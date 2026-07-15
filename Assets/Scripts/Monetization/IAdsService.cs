using System;
using UnityEngine;

namespace LuminaMatch.Monetization
{
    public interface IAdsService
    {
        bool IsRewardedReady { get; }
        void ShowRewarded(Action<bool> onCompleted);
        void ShowInterstitialIfAllowed();
    }

    /// <summary>
    /// Sandbox ads: instantly grants reward in Editor / without SDK keys.
    /// Replace body with Unity Ads / AdMob when store IDs are configured.
    /// </summary>
    public class SandboxAdsService : IAdsService
    {
        public bool IsRewardedReady => true;

        public void ShowRewarded(Action<bool> onCompleted)
        {
            Debug.Log("[LuminaMatch] Sandbox rewarded ad completed.");
            onCompleted?.Invoke(true);
        }

        public void ShowInterstitialIfAllowed()
        {
            Debug.Log("[LuminaMatch] Sandbox interstitial (skipped visually).");
        }
    }
}
