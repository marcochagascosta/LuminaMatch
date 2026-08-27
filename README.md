# Lumina Match

Match-3 mobile game (Unity C#) — restore the **Palácio de Luz**. Lives, boosters, continues, Unity IAP and AdMob included.

**Store target:** `0.1.41` · Android versionCode `49` · iOS build `50` · package `com.marcosaas.luminamatch`

## Requirements

- Unity **6000.5.x** (Unity 6)
- macOS **stable** for App Store archives (macOS beta → ITMS-90111)
- Android SDK via Unity Hub; release signing via env `LUMINA_STOREPASS` / `LUMINA_KEYPASS`

## Open & play

1. Open the project in Unity Hub
2. Menu **Lumina Match → Setup Project Scenes** (Boot scene + ids; keeps version 0.1.41)
3. Open `Assets/Scenes/Boot` and press **Play**

## Tests

Menu **Lumina Match → Run Edit Mode Tests Now**

## Monetization

- **Editor:** sandbox IAP/Ads (instant local grants)
- **Player / Release:** `UnityIapService` + `AdMobAdsService` — no free grants if the store/ad fails
- **Never upload** the Debug APK to stores (`DEVELOPMENT_BUILD` enables local IAP fallback)
- SKUs: `docs/STORE_IAP_SKUS.md` · Ads: `docs/STORE_ADS_SETUP.md` · Billing: `docs/BILLING_CHECKLIST.md`

## Builds

| Target | Menu | Output |
|--------|------|--------|
| Android AAB release | Build Android AAB (Release) | `Builds/Android/LuminaMatch-release.aab` |
| Android APK release | Build Android APK (Release) | `Builds/Android/LuminaMatch-release.apk` |
| iOS | Build iOS Xcode Project | `Builds/iOS/` → Archive in Xcode |

## Store publication

- Status: `docs/STORE_STATUS.md`
- Checklist: `docs/STORE_CHECKLIST.md`
- Listing copy: `docs/STORE_LISTING.md`
- Play promote: `python3 tools/play_promote_production.py --dry-run`
- iOS runbook: `docs/IOS_SUBMIT_RUNBOOK.md`

## Security

Never commit keystores, `.p12`, AuthKey `.p8`, or Play service accounts. See `.gitignore`.
