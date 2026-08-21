# Lumina Match — Store Status

**Branch:** `sync/0.1.41-store-ready`  
**Updated:** 2026-08-21 (GitHub sync)  
**Marketing version:** `0.1.41` · Android versionCode `49` · iOS buildNumber `50`

## Builds (ready locally)

| Artefato | Path | Status |
|----------|------|--------|
| Android APK | Desktop `LuminaMatch-0.1.3.apk` (= `Builds/Android/LuminaMatch-release.apk`) | `package=com.marcosaas.luminamatch`, `versionName=0.1.3`, `versionCode=4`, arm64, minSdk 26 — **sideload / adb** |
| Android AAB | Desktop `LuminaMatch-0.1.3.aab` (= `Builds/Android/LuminaMatch-release.aab`) | mesmo versionCode **4** — upload Play Internal Testing |
| iOS IPA | Desktop `LuminaMatch-build4.ipa` (0.1.2 / 4) | Archive OK neste Mac; upload App Store bloqueado por macOS beta |
| EditMode | 26/26 (pré-0.1.3) | Verde na sessão anterior |

## Install / Play (0.1.3)

- **adb:** nenhum device conectado na build de 2026-07-21 — instalar com:
  - `adb install -r ~/Desktop/LuminaMatch-0.1.3.apk`
  - (adb Unity:) `/Applications/Unity/Hub/Editor/6000.5.0f1/PlaybackEngines/AndroidPlayer/SDK/platform-tools/adb`
- **Play Internal Testing:** upload do AAB `~/Desktop/LuminaMatch-0.1.3.aab` (versionCode **4** > 3 já publicado).
- Internal testing: https://play.google.com/console/u/0/developers/6604076546202815303/app/4972110585725182702/tracks/internal-testing

## App Store Connect IAP (feito 2026-07-17 via API)

7 produtos criados + pt-BR/en-US + preço BRA + disponibilidade mundial + review screenshot.  
Estado: **READY_TO_SUBMIT** (submissão à review só junto com a versão do app).

| Product id | Preço BRA |
|------------|-----------|
| `...coins_small` | R$ 6,90 |
| `...coins_medium` | R$ 14,90 |
| `...coins_large` | R$ 39,90 |
| `...lives_refill` | R$ 6,90 |
| `...booster_pack` | R$ 14,90 |
| `...remove_ads` | R$ 49,90 |
| `...starter_pack` | R$ 9,90 |

## Play Console — publicado 2026-08-21

- App ID: `4972110585725182702` · Developer: `6604076546202815303`
- **Faixas ativas em 0.1.41 / versionCode 49** (`status: completed`):
  - `internal` (teste interno)
  - `alpha` (teste fechado)
- `production` e `beta` seguem vazias — subir só quando o lançamento aberto for decidido.
- Billing Library do artefato: **8.3.0** (`com.google.android.play.billingclient.version` no manifest).

### Publicar via API (sem clicar na Console)

Credencial: ADC do gcloud (`marko.formiga@gmail.com`) já com o escopo `androidpublisher`.
Projeto de cota: **`lumina-match-play47`** (API `androidpublisher.googleapis.com` habilitada em 2026-08-21).

Fluxo: `edits.insert` → upload do `.aab` em `/upload/.../bundles?uploadType=media` → `PUT tracks/{internal,alpha}` → `edits:validate` → `edits:commit`.
Cuidado no zsh: `"$VAR:commit"` vira `${VAR:c}`; usar `"${VAR}"':commit'`.

### Links

- Internal testing: https://play.google.com/console/u/0/developers/6604076546202815303/app/4972110585725182702/tracks/internal-testing
- Track releases: https://play.google.com/console/u/0/developers/6604076546202815303/app/4972110585725182702/tracks/4701303588687654607?tab=releases
- Produtos únicos (IAP): https://play.google.com/console/u/0/developers/6604076546202815303/app/4972110585725182702/one-time-products
- Monetização / merchant: https://play.google.com/console/u/0/developers/6604076546202815303/app/4972110585725182702/monetization-setup

## Play IAP

- **Criados 2026-08-15** (7 produtos únicos, opções de compra ativas).
- IDs ok: `coins_medium`, `coins_large`, `lives_refill`, `booster_pack`, `remove_ads`, `starter_pack`.
- **Alias Android:** `com.marcosaas.luminamatch.coins_small1` (Play criou com o `1`; o app mapeia só no Android).
- `remove_ads` no Brasil: **BRL 49,99** (tier Play; Apple R$ 49,90).

## AdMob

- Código no player: `AdMobAdsService`. IDs atuais = **sample do Google** (anúncio de teste, R$ 0).
- Colar App ID + unidades rewarded/interstitial reais em `Assets/Resources/Monetization/AdsConfig.json` para ganhar.

## Não enviar iOS deste Mac

macOS 27.0 beta → **ITMS-90111**. Archive App Store só em macOS estável.

## Ainda pendente (ação humana)

1. ~~Publicar nas faixas ativas~~ **FEITO 2026-08-21** (0.1.41 / 49 em `internal` + `alpha`).
2. ~~Criar 7 Play IAPs~~ **FEITO** (Android mapeia `coins_small1`).
3. Conferir na Play Console se o aviso da Biblioteca de Faturamento saiu (Google reavalia após a publicação).
4. Conta de comerciante Google Payments — sem ela o IAP real não cobra (`BILLING_CHECKLIST.md`).
5. iOS em Mac estável (macOS 27 beta dá ITMS-90111).

## Privacy

`Assets/Plugins/iOS/PrivacyInfo.xcprivacy` presente.

## Git note

Sincronizado em 2026-08-21. Fora do repo: `tmp_*.png`, `Builds/`, keystores, `Library/`.
