# Store submission checklist

## Before upload

- [x] Privacy Policy URL live (GitHub raw)
- [x] Age rating questionnaire (Apple) — preenchido via API
- [x] Icons 1024×1024
- [x] Screenshots iPhone 6.7" e 6.5" (marketing MVP)
- [x] Bundle ID `com.marcosaas.luminamatch` matches consoles
- [x] Version Android `0.1.1` / versionCode `2` · ASC version `1.0`
- [ ] Replace sandbox IAP/Ads with production Unity IAP + AdMob (antes de cobrança real)
- [ ] Screenshots de gameplay reais (opcional; marketing já enviado)
- [ ] iPad screenshots (opcional se só iPhone)

## App Store Connect

- [x] Create app record
- [x] Category Games / Puzzle
- [x] Description + privacy + copyright
- [x] Build TestFlight (build 2) — validado no iPhone
- [x] Build anexada à versão 1.0
- [ ] Contact phone App Review (confirme/edite no ASC)
- [ ] Submit for review

## Google Play Console

- [ ] Create app, complete Data safety form
- [ ] Upload AAB to Internal testing (`Desktop/LuminaMatch-release.aab`)
- [ ] Privacy policy URL
- [ ] Promote after QA OK

## Auth (do NOT paste passwords in chat)

- Apple: Xcode / Transporter / App Store Connect API key no Mac
- Google: Play Console no browser + keystore local
- GitHub: `gh auth login`
