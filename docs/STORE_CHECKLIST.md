# Store submission checklist — production `0.1.41`

Atualizado: 2026-08-27 · Branch `cursor/store-production-ready-d4b7`

## Before upload / promote

- [x] Privacy Policy URL live (GitHub raw)
- [x] Age rating / App Privacy (ASC)
- [x] Ícone 1024×1024 quadrado no repo (`docs/store/icon-1024.png`)
- [x] Feature graphic Play 1024×500 (`docs/store/feature-graphic-1024x500.png`)
- [x] Bundle ID `com.marcosaas.luminamatch`
- [x] Competitive gameplay + FTUE + powers + palace
- [x] Unity IAP 4.15.1 (Play Billing 8.3.0) + AdMob produção
- [x] AdMob IDs reais em `AdsConfig.json` (`STORE_ADS_SETUP.md`)
- [x] 7 IAP ASC READY_TO_SUBMIT + 7 IAP Play criados (`STORE_IAP_SKUS.md`)
- [x] Android AAB 0.1.41 / versionCode 49 em internal + alpha
- [ ] Screenshots de gameplay recentes colados nas lojas (Desktop → ASC/Play)
- [ ] Archive iOS em **macOS estável** (evitar ITMS-90111) e upload build 50
- [ ] Merchant Google Payments ativo (cobrança real Play)

## App Store Connect

- [x] App record + Games/Puzzle
- [x] Listing + privacy + copyright + free
- [x] TestFlight validado (build anterior)
- [x] Contact phone App Review
- [x] IAPs criados
- [ ] Nova binary 0.1.41 / build 50 anexada
- [ ] Submit for Review (app + IAPs juntos)

## Google Play Console

- [x] Internal + Closed (alpha) com 0.1.41 / 49
- [x] Listing + formulários + testers
- [x] Play IAPs criados
- [ ] Promover faixa `production` (script `tools/play_promote_production.py`)
- [ ] Confirmar aviso Billing Library 8 sumiu
- [ ] Merchant account OK

## Auth (nunca colar senhas no chat / git)

- Apple: ASC API key (.p8) ou Xcode/Transporter no Mac estável
- Google: ADC / service account com escopo `androidpublisher` (projeto `lumina-match-play47`)
- Android signing: env `LUMINA_STOREPASS` + `LUMINA_KEYPASS` (+ keystore local)
- GitHub: `gh auth login`
