using UnityEngine;

namespace LuminaMatch.Monetization
{
    public class MonetizationHub : MonoBehaviour
    {
        public static MonetizationHub Instance { get; private set; }

        /// <summary>
        /// Editor/dev defaults to sandbox. Player builds use production SDK paths
        /// (with local fallback until store IDs / dashboard are configured).
        /// </summary>
        public static bool UseProductionSdks =
#if UNITY_EDITOR
            false;
#else
            true;
#endif

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
                Debug.Log("[LuminaMatch] MonetizationHub: production SDK path.");
            }
            else
            {
                Iap = new SandboxIapService();
                Ads = new SandboxAdsService();
                Debug.Log("[LuminaMatch] MonetizationHub: sandbox.");
            }
        }
    }
}
