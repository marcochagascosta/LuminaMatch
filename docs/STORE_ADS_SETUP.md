# Store ads setup (Unity Ads)

Default ad network: **Unity Ads** (AdMob only if Unity blocks publication).

## Game IDs (placeholders — replace from Unity Dashboard)

| Platform | Game ID |
|----------|---------|
| iOS | `PLACEHOLDER_IOS_GAME_ID` |
| Android | `PLACEHOLDER_ANDROID_GAME_ID` |

## Placement IDs (placeholders)

| Placement | iOS | Android |
|-----------|-----|---------|
| Rewarded (continue / +1 vida) | `PLACEHOLDER_IOS_REWARDED` | `PLACEHOLDER_ANDROID_REWARDED` |
| Interstitial (pós-derrota / mapa) | `PLACEHOLDER_IOS_INTERSTITIAL` | `PLACEHOLDER_ANDROID_INTERSTITIAL` |

## Runtime selection

```csharp
// MonetizationHub.UseProductionSdks = false → SandboxAdsService (Editor/dev default)
// MonetizationHub.UseProductionSdks = true  → UnityAdsService stub (logs + simulates reward)
```

Wire real SDK init in `UnityAdsService` when IDs above are filled.

## Policy caps (implemented in UnityAdsService stub)

- **Rewarded:** continue flow and optional +1 vida na loja.
- **Interstitial:** máximo 1 a cada **180s**; nunca durante tutorial (`TutorialStep < 3`).
- **Remove Ads:** `PlayerSaveData.RemoveAds` bloqueia interstitials.

## Dashboard steps

1. Unity Dashboard → Monetization → create project Lumina Match.
2. Copy Game ID per platform into this doc and `UnityAdsService`.
3. Create Rewarded + Interstitial placements; paste placement IDs.
4. Enable test mode for internal builds; disable for production.
5. Link app in App Store Connect / Play Console ad network declarations if required.

## Testing

- Editor / dev: `SandboxAdsService` grants rewarded instantly.
- Device with `UseProductionSdks = true`: stub logs warning and invokes rewarded callback `true`.
