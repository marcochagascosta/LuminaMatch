using System;
using System.Collections.Generic;
using LuminaMatch.Economy;
using UnityEngine;

namespace LuminaMatch.Monetization
{
    public enum IapProductId
    {
        CoinsSmall,
        CoinsMedium,
        CoinsLarge,
        LivesRefill,
        BoosterPack,
        RemoveAds,
        StarterPack
    }

    public interface IIapService
    {
        bool IsReady { get; }
        string StatusText { get; }
        void Purchase(IapProductId id, Action<bool> onResult);
        void RestorePurchases(Action<bool> onResult);
        string GetPriceLabel(IapProductId id);
    }

    /// <summary>
    /// Grants products locally for Editor / sandbox testing.
    /// Wire Unity Purchasing (com.unity.purchasing) for production builds.
    /// </summary>
    public class SandboxIapService : IIapService
    {
        static readonly Dictionary<IapProductId, string> Prices = new()
        {
            { IapProductId.CoinsSmall, "R$ 6,90" },
            { IapProductId.CoinsMedium, "R$ 14,90" },
            { IapProductId.CoinsLarge, "R$ 39,90" },
            { IapProductId.LivesRefill, "R$ 6,90" },
            { IapProductId.BoosterPack, "R$ 14,90" },
            { IapProductId.RemoveAds, "R$ 49,90" },
            { IapProductId.StarterPack, "R$ 9,90" }
        };

        public bool IsReady => true;
        public string StatusText => "Sandbox (dev)";

        public string GetPriceLabel(IapProductId id)
            => Prices.TryGetValue(id, out var p) ? p : "—";

        public void Purchase(IapProductId id, Action<bool> onResult)
        {
            var progress = PlayerProgress.Instance;
            if (progress == null)
            {
                onResult?.Invoke(false);
                return;
            }

            IapGrants.Apply(id, progress);
            Debug.Log($"[LuminaMatch] Sandbox IAP granted: {id}");
            onResult?.Invoke(true);
        }

        public void RestorePurchases(Action<bool> onResult)
        {
            Debug.Log("[LuminaMatch] Sandbox restore — nothing to restore.");
            onResult?.Invoke(true);
        }
    }
}
