# Lumina Match — Store Status

**Branch:** `feat/competitive-store`  
**Updated:** 2026-07-16  
**Marketing version:** `0.1.2` · Android versionCode `3` · iOS build `4`

## Builds

- Android AAB: `Builds/Android/LuminaMatch-release.aab` (+ Desktop `LuminaMatch-0.1.2.aab`)
- EditMode: **26/26** passed (last verified)
- iOS Xcode project: export `Builds/iOS` for 0.1.2 / build 4 (in progress / verify Info.plist)

## Competitive redesign — on branch

- Royal Match art (gems, board, blockers, power VFX, palace stages)
- Sprite board + pre-level objectives + FTUE + palace home
- Board powers (rocket/bomb/color disk) + level overrides
- Idle hint + daily offer on home
- Starter + daily offers, `IapGrants`, Unity IAP/Ads in manifest
- Device builds use production SDK path; Editor stays sandbox
- Juice: concurrent punch/pop, palace drop-in, win reveal flash, SFX cues
- Batchmode: `SuppressMdrPrompt` avoids Unity Ads MDR dialog abort

## Store IDs still placeholders

Fill Unity Ads Game IDs in `UnityAdsService` / `docs/STORE_ADS_SETUP.md`.  
Create IAP products matching `docs/STORE_IAP_SKUS.md` in ASC + Play.

## Blockers

- App Store **ITMS-90111**: archive on **macOS estável** (not macOS 27 beta).
- Android: merchant account for paid IAP; upload AAB to internal when ready.

## Privacy

`Assets/Plugins/iOS/PrivacyInfo.xcprivacy` present.
