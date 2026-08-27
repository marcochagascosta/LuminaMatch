using System;
using System.Threading;
using LuminaMatch.Economy;
using UnityEngine;
using GoogleMobileAds.Api;

namespace LuminaMatch.Monetization
{
    /// <summary>
    /// AdMob rewarded + interstitial. IDs in Resources/Monetization/AdsConfig.json
    /// Google sample IDs show ads but pay $0 — replace with your AdMob units to earn.
    /// </summary>
    public class AdMobAdsService : IAdsService
    {
        static float _lastInterstitialRealtime;

        readonly SynchronizationContext _main;
        RewardedAd _rewarded;
        InterstitialAd _interstitial;
        bool _initialized;
        bool _initStarted;
        Action<bool> _pendingRewarded;

        public AdMobAdsService()
        {
            _main = SynchronizationContext.Current;
            if (AdsConfig.HasPlaceholderIds())
            {
                Debug.LogWarning("[LuminaMatch] AdMob App IDs missing — ads disabled.");
                return;
            }

            if (AdsConfig.IsGoogleSampleIds())
                Debug.LogWarning("[LuminaMatch] AdMob using Google sample IDs — ads work, no earnings.");

            try
            {
                MobileAds.RaiseAdEventsOnUnityMainThread = true;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[LuminaMatch] AdMob RaiseAdEventsOnUnityMainThread: {ex.Message}");
            }

            _initStarted = true;
            MobileAds.Initialize(_ =>
            {
                OnMain(() =>
                {
                    _initialized = true;
                    LoadRewarded();
                    LoadInterstitial();
                });
            });
        }

        public bool IsRewardedReady => _rewarded != null;

        public void ShowRewarded(Action<bool> onCompleted)
        {
            if (!_initStarted || AdsConfig.HasPlaceholderIds())
            {
                onCompleted?.Invoke(false);
                return;
            }

            if (IsRewardedReady)
            {
                ShowLoadedRewarded(onCompleted);
                return;
            }

            _pendingRewarded = onCompleted;
            if (_initialized)
                LoadRewarded();
        }

        public void ShowInterstitialIfAllowed()
        {
            var progress = PlayerProgress.Instance;
            if (progress != null && (progress.Data.TutorialStep < 3 || progress.Data.RemoveAds))
                return;

            if (Time.realtimeSinceStartup - _lastInterstitialRealtime < 180f)
                return;

            if (!_initialized || _interstitial == null)
                return;

            _lastInterstitialRealtime = Time.realtimeSinceStartup;
            var ad = _interstitial;
            _interstitial = null;
            ad.OnAdFullScreenContentClosed += () => OnMain(() =>
            {
                ad.Destroy();
                LoadInterstitial();
            });
            ad.OnAdFullScreenContentFailed += _ => OnMain(() =>
            {
                ad.Destroy();
                LoadInterstitial();
            });
            ad.Show();
        }

        void LoadRewarded()
        {
            RewardedAd.Load(AdsConfig.RewardedUnitId, new AdRequest(), (ad, error) =>
            {
                OnMain(() =>
                {
                    if (error != null || ad == null)
                    {
                        Debug.LogWarning($"[LuminaMatch] AdMob rewarded load failed: {error}");
                        var cb = _pendingRewarded;
                        _pendingRewarded = null;
                        cb?.Invoke(false);
                        return;
                    }

                    _rewarded = ad;
                    var pending = _pendingRewarded;
                    _pendingRewarded = null;
                    if (pending != null)
                        ShowLoadedRewarded(pending);
                });
            });
        }

        void LoadInterstitial()
        {
            InterstitialAd.Load(AdsConfig.InterstitialUnitId, new AdRequest(), (ad, error) =>
            {
                OnMain(() =>
                {
                    if (error != null || ad == null)
                    {
                        Debug.LogWarning($"[LuminaMatch] AdMob interstitial load failed: {error}");
                        return;
                    }
                    _interstitial = ad;
                });
            });
        }

        void ShowLoadedRewarded(Action<bool> onCompleted)
        {
            var ad = _rewarded;
            _rewarded = null;
            if (ad == null)
            {
                onCompleted?.Invoke(false);
                LoadRewarded();
                return;
            }

            bool rewarded = false;
            ad.OnAdFullScreenContentClosed += () => OnMain(() =>
            {
                ad.Destroy();
                onCompleted?.Invoke(rewarded);
                LoadRewarded();
            });
            ad.OnAdFullScreenContentFailed += _ => OnMain(() =>
            {
                ad.Destroy();
                onCompleted?.Invoke(false);
                LoadRewarded();
            });
            ad.Show(_ => { rewarded = true; });
        }

        void OnMain(Action action)
        {
            if (action == null) return;
            if (_main == null)
                action();
            else
                _main.Post(_ => action(), null);
        }
    }
}
