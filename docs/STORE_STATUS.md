# Lumina Match — Store Status

**Branch:** `feat/competitive-store`  
**Updated:** 2026-07-16

## Competitive redesign (in progress → playable)

Delivered on branch (see plan `docs/superpowers/plans/2026-07-16-lumina-competitive-store.md`):

- Royal Match art: gem sprites, board frame/cells, blockers, power VFX, palace stages 0–2
- Board UI uses sprites via `ArtCatalog` + palace keyframes on Home
- Board powers: rocket (4), bomb (5), color disk (L/T) + resolve/activate
- Level overrides for 1–10 / milestones; softer tutorial levels
- FTUE hints (tutorial step on save)
- Starter pack + daily offer rules (`OfferService`)
- `IapGrants` + `StarterPack` SKU; Unity IAP/Ads stubs (`UseProductionSdks`)
- Docs: `STORE_IAP_SKUS.md`, `STORE_ADS_SETUP.md`, art prompts

**Still needed for “cobrar de verdade”:**

1. Wire real Unity Purchasing + Unity Ads Game IDs (`UseProductionSdks = true`)
2. Create store IAP products with SKUs in docs
3. Play Mode QA on device
4. iOS archive on **macOS estável** (not 27 beta) — ITMS-90111
5. Optional: more Meshy 3D palace stages replacing 2D keyframes

## Prior store notes

- Play internal test AAB was uploaded earlier (versionCode 2 / 0.1.1)
- App Store `0.1.1` hit INVALID_BINARY (ITMS-90111 Unsupported SDK / macOS beta build machine)
- PrivacyInfo.xcprivacy exists under `Assets/Plugins/iOS/`

## Marketing version

Keep `0.1.1` until competitive build ships; bump iOS `CFBundleVersion` / Android versionCode on next store upload.
