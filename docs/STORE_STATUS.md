# Lumina Match — Store Status

**Branch:** `cursor/store-production-ready-d4b7`  
**Updated:** 2026-08-27  
**Marketing:** `0.1.42` · Android versionCode **49** (promote AAB já publicado) · iOS build **51** (fix iPad + CI)

## Faturar agora (sem Macbook beta)

Ver **`docs/LAUNCH_NOW.md`**.

| Canal | Ação | Status |
|-------|------|--------|
| Play Production | Workflow `Promote Play Production` ou `tools/play_promote_production.py --yes` | Aguardando secret `GOOGLE_APPLICATION_CREDENTIALS_JSON` |
| iOS TestFlight | Workflow `iOS TestFlight (macOS CI)` em runner macOS-14 | Aguardando UNITY_* + ASC_* |
| Merchant Play | Console monetization-setup | Confirmar (bloqueia cobrança IAP) |

## Já feito

- Código competitive + IAP/AdMob + fix UI iPad (scale 0.9)
- AAB **0.1.41 / 49** em Play `internal` + `alpha`
- 7 IAPs ASC + 7 IAPs Play
- Workflows CI em `.github/workflows/`
- Listing / privacy / feature graphic no repo

## Macbook beta

Não usar para upload iOS (ITMS-90111). Usar o workflow macOS CI.

## Links

- Play Production: https://play.google.com/console/u/0/developers/6604076546202815303/app/4972110585725182702/tracks/production
- ASC: https://appstoreconnect.apple.com/apps/6791448071/appstore
- PR: https://github.com/marcochagascosta/LuminaMatch/pull/4
