# Lumina Match — Store Status

**Branch:** `feat/competitive-store`  
**Updated:** 2026-07-16  
**Marketing version:** `0.1.2` · Android versionCode `3` · iOS build `4`

## Builds (ready locally)

| Artefato | Path | Status |
|----------|------|--------|
| Android AAB | `Builds/Android/LuminaMatch-release.aab` + Desktop `LuminaMatch-0.1.2.aab` | Pronto p/ Play internal |
| iOS IPA | Desktop `LuminaMatch-build4.ipa` (0.1.2 / 4, ~96 MB, com Unity Ads pods + IAP capability) | Archive OK neste Mac |
| EditMode | 26/26 | Verde |

## Competitive redesign — on branch

- Royal Match art, board presenter, FTUE, powers, palace stages
- Idle hint + daily/starter offers, Unity IAP/Ads packages
- Juice (punch/drop/flash) + SFX cues + win palace reveal
- Batchmode: `SuppressMdrPrompt`; CocoaPods instalado (brew) p/ UnityAds
- ASC App ID: capability **IN_APP_PURCHASE** habilitada via API

## Não enviar iOS deste Mac

Este host é **macOS 27.0 beta** (`26A5378n`). Upload → **ITMS-90111** de novo.  
Archive/upload App Store só em **macOS estável** (copiar projeto `Builds/iOS` ou IPA rebuild lá).

## Próximos passos loja

1. Play Console: upload AAB `0.1.2` (versionCode 3) no teste interno
2. Criar produtos IAP (`docs/STORE_IAP_SKUS.md`) em ASC + Play
3. Preencher Unity Ads Game IDs (`UnityAdsService` / `docs/STORE_ADS_SETUP.md`)
4. Conta merchant Google p/ IAP pagos
5. iOS: archive em Mac estável → TestFlight

## Privacy

`Assets/Plugins/iOS/PrivacyInfo.xcprivacy` presente (copiado p/ `Builds/iOS` no export).
