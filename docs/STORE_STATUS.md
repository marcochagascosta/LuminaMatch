# Lumina Match — Store Status

**Branch:** `feat/competitive-store`  
**Updated:** 2026-07-16  
**Marketing version (next upload):** `0.1.2` · Android versionCode `3` · iOS build `4`

## Competitive redesign — done on branch

- Royal Match art (gems, board, blockers, power VFX, palace stages)
- Sprite board + pre-level objectives + FTUE + palace home
- Board powers (rocket/bomb/color disk) + level overrides
- Starter + daily offers, `IapGrants`, Unity IAP/Ads packages in manifest
- Player builds use production SDK path (`UseProductionSdks`); Editor stays sandbox
- Docs: SKUs, ads setup, GDD updated, art prompts
- EditMode: **25/25** green (last verified)

## Store IDs still placeholders

Fill Unity Ads Game IDs in `UnityAdsService` / `docs/STORE_ADS_SETUP.md`.  
Create IAP products matching `docs/STORE_IAP_SKUS.md` in ASC + Play.

## Prior blockers

- App Store INVALID_BINARY **ITMS-90111**: archive on **macOS estável** (not macOS 27 beta).
- Android: validate install when device available; merchant account for paid IAP.

## Privacy

`Assets/Plugins/iOS/PrivacyInfo.xcprivacy` present.
