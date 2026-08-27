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
    /// Production IAP via Unity Purchasing. Store product ids: docs/STORE_IAP_SKUS.md
    /// Release builds never grant items without a store receipt.
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

        /// <summary>
        /// Play Console created coins_small as coins_small1 (IDs are immutable).
        /// Apple keeps the canonical id; Android talks to the Play SKU that exists.
        /// </summary>
        const string CoinsSmallPlayId = "com.marcosaas.luminamatch.coins_small1";

        public static string GetStoreId(IapProductId id)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (id == IapProductId.CoinsSmall)
                return CoinsSmallPlayId;
#endif
            return StoreIds[id];
        }

        static readonly Dictionary<IapProductId, string> FallbackPrices = new()
        {
            { IapProductId.CoinsSmall, "R$ 6,90" },
            { IapProductId.CoinsMedium, "R$ 14,90" },
            { IapProductId.CoinsLarge, "R$ 39,90" },
            { IapProductId.LivesRefill, "R$ 6,90" },
            { IapProductId.BoosterPack, "R$ 14,90" },
            { IapProductId.RemoveAds, "R$ 49,90" },
            { IapProductId.StarterPack, "R$ 9,90" }
        };

        /// <summary>Only Editor / Development builds may grant without a store receipt.</summary>
        public static bool AllowLocalFallback =>
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            true;
#else
            false;
#endif

#if UNITY_PURCHASING
        IStoreController _controller;
        IExtensionProvider _extensions;
        Action<bool> _pendingCallback;
        IapProductId _pendingId;
        string _initError;
#endif

        public bool IsReady
        {
            get
            {
#if UNITY_PURCHASING
                return _controller != null;
#else
                return AllowLocalFallback;
#endif
            }
        }

        public string StatusText
        {
            get
            {
#if UNITY_PURCHASING
                if (_controller != null) return "Loja pronta";
                if (!string.IsNullOrEmpty(_initError)) return $"Loja: {_initError}";
                return "Conectando à loja…";
#else
                return AllowLocalFallback ? "IAP local (dev)" : "IAP indisponível";
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
                builder.AddProduct(GetStoreId(kv.Key), type);
            }
            UnityPurchasing.Initialize(this, builder);
#else
            Debug.LogWarning("[LuminaMatch] com.unity.purchasing not resolved.");
#endif
        }

        public string GetPriceLabel(IapProductId id)
        {
#if UNITY_PURCHASING
            if (_controller != null)
            {
                var storeId = GetStoreId(id);
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
            if (_controller == null)
            {
                if (AllowLocalFallback)
                {
                    GrantLocal(id, onResult);
                    return;
                }
                Debug.LogWarning("[LuminaMatch] IAP not ready — purchase blocked (no free grant).");
                onResult?.Invoke(false);
                return;
            }

            var storeId = GetStoreId(id);
            var product = _controller.products.WithID(storeId);
            if (product == null || !product.availableToPurchase)
            {
                if (AllowLocalFallback)
                {
                    Debug.LogWarning($"[LuminaMatch] Product unavailable ({storeId}) — local grant (dev only).");
                    GrantLocal(id, onResult);
                    return;
                }
                Debug.LogWarning($"[LuminaMatch] Product unavailable ({storeId}) — purchase blocked.");
                onResult?.Invoke(false);
                return;
            }

            _pendingCallback = onResult;
            _pendingId = id;
            _controller.InitiatePurchase(product);
#else
            if (AllowLocalFallback)
                GrantLocal(id, onResult);
            else
                onResult?.Invoke(false);
#endif
        }

        public void RestorePurchases(Action<bool> onResult)
        {
#if UNITY_PURCHASING
            if (_controller == null || _extensions == null)
            {
                onResult?.Invoke(false);
                return;
            }

            // Google Play re-delivers owned non-consumables via ProcessPurchase on init.
            // Apple needs an explicit restore.
            bool any = ReapplyOwnedEntitlements();
#if UNITY_IOS || UNITY_IPHONE
            var apple = _extensions.GetExtension<IAppleExtensions>();
            if (apple != null)
            {
                apple.RestoreTransactions(success =>
                {
                    ReapplyOwnedEntitlements();
                    onResult?.Invoke(success || any);
                });
                return;
            }
#endif
            onResult?.Invoke(any);
#else
            onResult?.Invoke(AllowLocalFallback);
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
        bool ReapplyOwnedEntitlements()
        {
            if (_controller == null || PlayerProgress.Instance == null) return false;
            bool any = false;
            foreach (var product in _controller.products.all)
            {
                if (product == null || !product.hasReceipt) continue;
                if (product.definition.type != ProductType.NonConsumable) continue;
                var id = FindId(product.definition.id);
                if (!id.HasValue) continue;
                IapGrants.Apply(id.Value, PlayerProgress.Instance);
                any = true;
            }
            return any;
        }

        static IapProductId? FindId(string storeId)
        {
            if (storeId == CoinsSmallPlayId)
                return IapProductId.CoinsSmall;
            foreach (var kv in StoreIds)
                if (kv.Value == storeId) return kv.Key;
            return null;
        }

        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            _controller = controller;
            _extensions = extensions;
            _initError = null;
            ReapplyOwnedEntitlements();
            Debug.Log("[LuminaMatch] Unity IAP initialized.");
        }

        public void OnInitializeFailed(InitializationFailureReason error)
        {
            _initError = error.ToString();
            Debug.LogError($"[LuminaMatch] Unity IAP init failed: {error}");
        }

        public void OnInitializeFailed(InitializationFailureReason error, string message)
        {
            _initError = string.IsNullOrEmpty(message) ? error.ToString() : message;
            Debug.LogError($"[LuminaMatch] Unity IAP init failed: {error} {message}");
        }

        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
        {
            var matched = FindId(args.purchasedProduct.definition.id);
            if (matched.HasValue && PlayerProgress.Instance != null)
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
