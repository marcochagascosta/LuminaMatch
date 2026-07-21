# Lumina Match — Store Status

**Branch:** `feat/competitive-store` @ `2b49270`  
**Updated:** 2026-07-21 (build 0.1.3 local)  
**Marketing version:** `0.1.3` · Android versionCode `4` · iOS build `5` (ProjectSettings / BuildScripts)

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
| `...remove_ads` | R$ 19,90 |
| `...starter_pack` | R$ 9,90 |

## Play Console — Internal Testing (PUBLICADO 2026-07-18 ~23:47 — ainda 0.1.2)

- App ID: `4972110585725182702` · Developer: `6604076546202815303`
- **Status (último publicado):** Disponível para testadores internos — **0.1.2 / code 3**
- Próximo: publicar **0.1.3 / code 4** via AAB acima

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
2. **Upload Internal Testing 0.1.3** (code 4) — AAB no Desktop **ou** sideload APK via USB.
3. Configurar conta merchant Google Payments → criar 7 Play IAPs.
4. Unity Ads Game IDs.
5. iOS em Mac estável.

## Privacy

`Assets/Plugins/iOS/PrivacyInfo.xcprivacy` presente.

## Git note

Não commitar: `Assets/Scenes/Boot.unity`, `Assets/MobileDependencyResolver/**`, `ProjectSettings/GvhProjectSettings.xml` (ruído/resolver; sem segredos, mas fora do escopo store docs).
