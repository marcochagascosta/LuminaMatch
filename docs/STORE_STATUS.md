# Lumina Match — Store Status

**Branch:** `feat/competitive-store`  
**Updated:** 2026-07-18 (sessão 3 — Internal Testing publicado)  
**Marketing version:** `0.1.2` · Android versionCode `3` · iOS build `4`

## Builds (ready locally)

| Artefato | Path | Status |
|----------|------|--------|
| Android AAB | Desktop `LuminaMatch-0.1.2.aab` (= `Builds/Android/LuminaMatch-release.aab`) | `package=com.marcosaas.luminamatch`, `versionName=0.1.2`, `versionCode=3` — **já na biblioteca Play** |
| iOS IPA | Desktop `LuminaMatch-build4.ipa` (0.1.2 / 4) | Archive OK neste Mac; upload App Store bloqueado por macOS beta |
| EditMode | 26/26 | Verde |

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
| `...remove_ads` | R$ 19,90 |
| `...starter_pack` | R$ 9,90 |

## Play Console — Internal Testing (PUBLICADO 2026-07-18 ~23:47)

- App ID: `4972110585725182702` · Developer: `6604076546202815303`
- **Status: Disponível para testadores internos**
- Release name: `0.1.2 e código 3`
- Artifact: App Bundle **3 (0.1.2)** — ABI `arm64-v8a`, minSdk 26, targetSdk 36
- Notas pt-BR: build competitivo store / IAP-Ads / teste interno
- **Rebuild NÃO foi necessário** — reupload do AAB falhou com “código de versão 3 já foi usado”; o artefato foi **anexado da biblioteca** e publicado via “Salvar e publicar”

### Device compatibility note

- Aviso antigo de ~12.479 devices dropados vs stub/build anterior: esperado em primeiro AAB “de verdade” (minSdk 26 + ARM64-only).
- Na review desta release vs `2 (0.1.1)`: tabela mostrou **0** dispositivos perdidos (mesmo ABI/minSdk). Avisos restantes = tamanho APK, falta mapping/símbolos nativos (não bloqueiam Internal).

### Automação usada

- Chrome AppleScript + `execute javascript` (funcionou nesta sessão).
- Fluxo: biblioteca → bundle 3 → Avançar → Salvar e publicar → confirmar diálogo.
- Android Publisher API ainda bloqueada (ADC sem quota project GCP / user token sem scope). Helper Desktop permanece útil para IAPs pós-merchant.

### Links

- Internal testing: https://play.google.com/console/u/0/developers/6604076546202815303/app/4972110585725182702/tracks/internal-testing
- Track releases: https://play.google.com/console/u/0/developers/6604076546202815303/app/4972110585725182702/tracks/4701303588687654607?tab=releases
- Produtos únicos (IAP): https://play.google.com/console/u/0/developers/6604076546202815303/app/4972110585725182702/one-time-products
- Monetização / merchant: https://play.google.com/console/u/0/developers/6604076546202815303/app/4972110585725182702/monetization-setup

## Play IAP

- **Não criados** — página bloqueada: *“É preciso configurar uma conta do comerciante do Google Payments”*.
- SKUs alvo = `docs/STORE_IAP_SKUS.md` (mesmos IDs da Apple).
- Próximo clique óbvio: **Configure uma conta de comerciante** em Monetização (requer conta Google Payments do Marco; não automatizável sem dados bancários/ID).

## Unity Ads

- **Pendente.** Sessão Unity Dashboard não autenticada.

## Não enviar iOS deste Mac

macOS 27.0 beta → **ITMS-90111**. Archive App Store só em macOS estável.

## Ainda pendente (ação humana)

1. ~~Internal Testing 0.1.2~~ **FEITO** (code 3).
2. Configurar conta merchant Google Payments → criar 7 Play IAPs.
3. Unity Ads Game IDs.
4. iOS em Mac estável.

## Privacy

`Assets/Plugins/iOS/PrivacyInfo.xcprivacy` presente.

## Git note

Não commitar: `Assets/Scenes/Boot.unity`, `Assets/MobileDependencyResolver/**`, `ProjectSettings/GvhProjectSettings.xml` (ruído/resolver; sem segredos, mas fora do escopo store docs).
