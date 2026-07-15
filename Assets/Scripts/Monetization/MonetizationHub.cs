using UnityEngine;

namespace LuminaMatch.Monetization
{
    public class MonetizationHub : MonoBehaviour
    {
        public static MonetizationHub Instance { get; private set; }

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
            Iap = new SandboxIapService();
            Ads = new SandboxAdsService();
        }
    }
}
