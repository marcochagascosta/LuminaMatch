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
        RemoveAds
    }

    public interface IIapService
    {
        bool IsReady { get; }
        void Purchase(IapProductId id, Action<bool> onResult);
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
            { IapProductId.RemoveAds, "R$ 19,90" }
        };

        public bool IsReady => true;

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

            switch (id)
            {
                case IapProductId.CoinsSmall:
                    progress.AddCoins(500);
                    break;
                case IapProductId.CoinsMedium:
                    progress.AddCoins(1500);
                    break;
                case IapProductId.CoinsLarge:
                    progress.AddCoins(5000);
                    break;
                case IapProductId.LivesRefill:
                    progress.AddLives(progress.Data.MaxLives);
                    break;
                case IapProductId.BoosterPack:
                    progress.AddBooster(BoosterType.Hammer, 3);
                    progress.AddBooster(BoosterType.Swap, 3);
                    progress.AddBooster(BoosterType.LineBlast, 3);
                    break;
                case IapProductId.RemoveAds:
                    progress.GrantRemoveAds();
                    break;
            }

            Debug.Log($"[LuminaMatch] Sandbox IAP granted: {id}");
            onResult?.Invoke(true);
        }
    }
}
