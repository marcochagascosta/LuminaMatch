# Store IAP SKUs

Bundle ID / package: `com.marcosaas.luminamatch`

Map `IapProductId` → store product id (same on App Store Connect and Google Play).

| IapProductId | Store product id | Type | Grant (via IapGrants) |
|--------------|------------------|------|------------------------|
| CoinsSmall | `com.marcosaas.luminamatch.coins_small` (Play Android: `...coins_small1`) | Consumable | +500 moedas |
| CoinsMedium | `com.marcosaas.luminamatch.coins_medium` | Consumable | +1500 moedas |
| CoinsLarge | `com.marcosaas.luminamatch.coins_large` | Consumable | +5000 moedas |
| LivesRefill | `com.marcosaas.luminamatch.lives_refill` | Consumable | Enche vidas (max 5) |
| BoosterPack | `com.marcosaas.luminamatch.booster_pack` | Consumable | +3 de cada booster |
| RemoveAds | `com.marcosaas.luminamatch.remove_ads` | Non-consumable | `RemoveAds = true` |
| StarterPack | `com.marcosaas.luminamatch.starter_pack` | Non-consumable | +2000 moedas, +5 boosters, vidas cheias, `StarterPackBought` |

## Sandbox / dev

- `MonetizationHub.UseProductionSdks = false` → `SandboxIapService` (instant local grants).
- `UseProductionSdks = true` → `UnityIapService` (Unity Purchasing **4.15.1**, Google Play Billing Library **8.3.0**).
- **Release:** se produto/loja indisponível → compra falha, **sem grant grátis**.
- **Editor / DEVELOPMENT_BUILD:** fallback local ainda permitido.
- Soft currency (vidas/boosters com moedas): `SoftShop` — ver `docs/BILLING_CHECKLIST.md`.

## ASC / Play Console checklist

1. Create each product with ids above (localized title/description pt-BR + en).
2. Starter Pack: one-time offer; show via `OfferService.ShouldShowStarterPack`.
3. Submit pricing tier before review (BRL primary).
4. Link entitlements to `IapGrants.Apply` in `UnityIapService` purchase callback when SDK is live.
