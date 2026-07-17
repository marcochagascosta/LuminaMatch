using System;
using System.Collections.Generic;
using LuminaMatch.Economy;
using UnityEngine;

namespace LuminaMatch.Monetization
{
    /// <summary>
    /// Production IAP stub. Wire Unity Purchasing (com.unity.purchasing) and map store IDs
    /// from docs/STORE_IAP_SKUS.md before shipping.
    /// </summary>
    public class UnityIapService : IIapService
    {
        static readonly Dictionary<IapProductId, string> FallbackPrices = new()
        {
            { IapProductId.CoinsSmall, "R$ 6,90" },
            { IapProductId.CoinsMedium, "R$ 14,90" },
            { IapProductId.CoinsLarge, "R$ 39,90" },
            { IapProductId.LivesRefill, "R$ 6,90" },
            { IapProductId.BoosterPack, "R$ 14,90" },
            { IapProductId.RemoveAds, "R$ 19,90" },
            { IapProductId.StarterPack, "R$ 9,90" }
        };

        public bool IsReady => false;

        public string GetPriceLabel(IapProductId id)
            => FallbackPrices.TryGetValue(id, out var p) ? p : "—";

        public void Purchase(IapProductId id, Action<bool> onResult)
        {
            Debug.LogWarning("[LuminaMatch] Unity IAP not configured — granting locally. Configure store IDs per docs/STORE_IAP_SKUS.md.");

            var progress = PlayerProgress.Instance;
            if (progress == null)
            {
                onResult?.Invoke(false);
                return;
            }

            IapGrants.Apply(id, progress);
            Debug.Log($"[LuminaMatch] Unity IAP stub granted: {id}");
            onResult?.Invoke(true);
        }
    }
}
