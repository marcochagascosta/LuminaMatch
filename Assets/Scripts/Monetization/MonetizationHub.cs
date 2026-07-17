using UnityEngine;

namespace LuminaMatch.Monetization
{
    public class MonetizationHub : MonoBehaviour
    {
        public static MonetizationHub Instance { get; private set; }

        /// <summary>When true, use Unity IAP/Ads stubs instead of sandbox services.</summary>
        public static bool UseProductionSdks = false;

        public IIapService Iap { get; private set; }
        public IAdsService Ads { get; private set; }

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (UseProductionSdks)
            {
                Iap = new UnityIapService();
                Ads = new UnityAdsService();
            }
            else
            {
                Iap = new SandboxIapService();
                Ads = new SandboxAdsService();
            }
        }
    }
}
