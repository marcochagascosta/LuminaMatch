# Store submission — status 2026-07-16 (atualizado)

## Pronto neste Mac

| Artefato | Caminho |
|----------|---------|
| Android AAB (release) | Desktop `LuminaMatch-release.aab` (~18 MB) |
| Android APK (debug) | Desktop `LuminaMatch-debug.apk` |
| iOS IPA (App Store) | Desktop `LuminaMatch.ipa` |
| Ícone 1024 | Desktop `LuminaMatch-icon-1024.png` |
| Screenshots marketing | Desktop `LuminaMatch-screenshots/` |
| Privacy policy | `docs/PRIVACY_POLICY.md` |
| Copy da loja | `docs/STORE_LISTING.md` |
| Keystore Android | `~/.lumina-match-secrets/` (fora do git) |

- Bundle ID: `com.marcosaas.luminamatch`
- Versão Unity/Android: `0.1.1` (versionCode `2`)
- App Store Connect version: `1.0` (estado PREPARE_FOR_SUBMISSION)
- Apple app id: `6791448071`
- Team Apple: `6LQQD54JHB`
- Privacy URL: https://raw.githubusercontent.com/marcochagascosta/LuminaMatch/main/docs/PRIVACY_POLICY.md

## iOS — feito

- [x] App criado no App Store Connect
- [x] IPA enviada (Transporter) — build **2** VALID
- [x] TestFlight interno — **confirmado no iPhone do Marco**
- [x] Build anexada à versão 1.0
- [x] Nome, subtítulo, descrição pt-BR, keywords, support URL
- [x] Privacy Policy URL
- [x] Copyright `2026 Marco Costa`
- [x] Screenshots 6.7" e 6.5" (3 cada) enviados via API

## iOS — falta para enviar à Review

1. Classificação etária (questionário Age Rating) no ASC — API pediu campos extras; complete no browser se ainda vermelho
2. Categoria Games → Puzzle (tente no UI se API falhar)
3. Contact phone do App Review (formato `+55 …`)
4. Ícone da loja se ainda não estiver no asset catalog / ASC
5. Quando checklist verde: **Add for Review** → **Submit**

Link: https://appstoreconnect.apple.com/apps/6791448071/appstore

## Google Play — próximo (manual no browser)

Não há service account Play neste Mac; upload é no Console.

1. Abra [Play Console](https://play.google.com/console) (já aberto se o `open` rodou)
2. **Create app** → nome `Lumina Match` → Free → declare políticas
3. **Dashboard** complete os itens obrigatórios (Privacy policy URL acima)
4. **App content** → Data safety: só progresso local / IAP via Google Play; sem coleta de PII
5. **Testing → Internal testing → Create new release**
6. Upload: `Desktop/LuminaMatch-release.aab` (caminho também na área de transferência)
7. Adicione-se como tester → instale pelo link Internal
8. Depois do OK: promover para Closed/Production

Package: `com.marcosaas.luminamatch`

## Monetização (ainda sandbox)

IAP/Ads no código são sandbox. Antes de produção real com cobrança:
- Unity IAP + produtos reais nas lojas
- AdMob / Unity Ads com IDs de produção
