using System;
using System.Collections.Generic;
using LuminaMatch.Economy;
using UnityEngine;
#if UNITY_PURCHASING
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;
#endif

namespace LuminaMatch.Monetization
{
    /// <summary>
    /// Production IAP via Unity Purchasing when package is present; otherwise local grant fallback.
    /// Store product ids: docs/STORE_IAP_SKUS.md
    /// </summary>
    public class UnityIapService : IIapService
#if UNITY_PURCHASING
        , IDetailedStoreListener
#endif
    {
        public static readonly Dictionary<IapProductId, string> StoreIds = new()
        {
            { IapProductId.CoinsSmall, "com.marcosaas.luminamatch.coins_small" },
            { IapProductId.CoinsMedium, "com.marcosaas.luminamatch.coins_medium" },
            { IapProductId.CoinsLarge, "com.marcosaas.luminamatch.coins_large" },
            { IapProductId.LivesRefill, "com.marcosaas.luminamatch.lives_refill" },
            { IapProductId.BoosterPack, "com.marcosaas.luminamatch.booster_pack" },
            { IapProductId.RemoveAds, "com.marcosaas.luminamatch.remove_ads" },
            { IapProductId.StarterPack, "com.marcosaas.luminamatch.starter_pack" }
        };

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

#if UNITY_PURCHASING
        IStoreController _controller;
        IExtensionProvider _extensions;
        Action<bool> _pendingCallback;
        IapProductId _pendingId;
#endif

        public bool IsReady
        {
            get
            {
#if UNITY_PURCHASING
                return _controller != null;
#else
                return true;
#endif
            }
        }

        public UnityIapService()
        {
#if UNITY_PURCHASING
            var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
            foreach (var kv in StoreIds)
            {
                var type = kv.Key is IapProductId.RemoveAds or IapProductId.StarterPack
                    ? ProductType.NonConsumable
                    : ProductType.Consumable;
                builder.AddProduct(kv.Value, type);
            }
            UnityPurchasing.Initialize(this, builder);
#else
            Debug.LogWarning("[LuminaMatch] com.unity.purchasing not resolved — UnityIapService using local grants.");
#endif
        }

        public string GetPriceLabel(IapProductId id)
        {
#if UNITY_PURCHASING
            if (_controller != null && StoreIds.TryGetValue(id, out var storeId))
            {
                var product = _controller.products.WithID(storeId);
                if (product != null && product.availableToPurchase && product.metadata != null
                    && !string.IsNullOrEmpty(product.metadata.localizedPriceString))
                    return product.metadata.localizedPriceString;
            }
#endif
            return FallbackPrices.TryGetValue(id, out var p) ? p : "—";
        }

        public void Purchase(IapProductId id, Action<bool> onResult)
        {
#if UNITY_PURCHASING
            if (_controller == null || !StoreIds.TryGetValue(id, out var storeId))
            {
                GrantLocal(id, onResult);
                return;
            }

            var product = _controller.products.WithID(storeId);
            if (product == null || !product.availableToPurchase)
            {
                Debug.LogWarning($"[LuminaMatch] Product unavailable ({storeId}) — local grant fallback.");
                GrantLocal(id, onResult);
                return;
            }

            _pendingCallback = onResult;
            _pendingId = id;
            _controller.InitiatePurchase(product);
#else
            GrantLocal(id, onResult);
#endif
        }

        static void GrantLocal(IapProductId id, Action<bool> onResult)
        {
            var progress = PlayerProgress.Instance;
            if (progress == null)
            {
                onResult?.Invoke(false);
                return;
            }
            IapGrants.Apply(id, progress);
            onResult?.Invoke(true);
        }

#if UNITY_PURCHASING
        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            _controller = controller;
            _extensions = extensions;
            Debug.Log("[LuminaMatch] Unity IAP initialized.");
        }

        public void OnInitializeFailed(InitializationFailureReason error)
            => Debug.LogError($"[LuminaMatch] Unity IAP init failed: {error}");

        public void OnInitializeFailed(InitializationFailureReason error, string message)
            => Debug.LogError($"[LuminaMatch] Unity IAP init failed: {error} {message}");

        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
        {
            var storeId = args.purchasedProduct.definition.id;
            IapProductId? matched = null;
            foreach (var kv in StoreIds)
            {
                if (kv.Value == storeId)
                {
                    matched = kv.Key;
                    break;
                }
            }

            if (matched.HasValue)
                IapGrants.Apply(matched.Value, PlayerProgress.Instance);

            _pendingCallback?.Invoke(true);
            _pendingCallback = null;
            return PurchaseProcessingResult.Complete;
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
        {
            Debug.LogWarning($"[LuminaMatch] Purchase failed: {product?.definition?.id} {failureReason}");
            _pendingCallback?.Invoke(false);
            _pendingCallback = null;
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
        {
            Debug.LogWarning($"[LuminaMatch] Purchase failed: {failureDescription?.message}");
            _pendingCallback?.Invoke(false);
            _pendingCallback = null;
        }
#endif
    }
}
