# Store submission — status 2026-07-16

## Pronto neste Mac

| Artefato | Caminho |
|----------|---------|
| Android AAB (release) | `Builds/Android/LuminaMatch-release.aab` e Desktop |
| Android APK (debug) | Desktop `LuminaMatch-debug.apk` |
| iOS IPA (App Store) | `Builds/Export/LuminaMatch.ipa` e Desktop |
| Ícone 1024 | Desktop `LuminaMatch-icon-1024.png` |
| Privacy policy | `docs/PRIVACY_POLICY.md` |
| Copy da loja | `docs/STORE_LISTING.md` |
| Keystore Android | `~/.lumina-match-secrets/` (fora do git) |

Bundle ID: `com.marcosaas.luminamatch`  
Versão: `0.1.1` (build/versionCode `2`)  
Team Apple: `6LQQD54JHB`

## Google Play (próximo clique)

1. [Play Console](https://play.google.com/console) → Create app → Lumina Match
2. Preencha Data safety (local progress only; IAP via Google)
3. Testing → Internal testing → Create release → upload `LuminaMatch-release.aab`
4. Cole privacy URL (raw GitHub após push) e ícone

## App Store / TestFlight

IPA já exportada. Upload automático via API key do ambiente retornou **401 NOT_AUTHORIZED** (Issuer/Key inválidos ou sem role).

Faça no Mac (1 minuto):

1. Abra **Transporter** ou Xcode → Organizer → distribute o archive  
   `Builds/Archive/LuminaMatch.xcarchive`
2. Ou Transporter → deliver `Desktop/LuminaMatch.ipa`
3. Em [App Store Connect](https://appstoreconnect.apple.com): crie o app se ainda não existir (bundle `com.marcosaas.luminamatch`), complete listing + age rating, depois TestFlight

Se quiser upload por CLI depois: atualize `ASC_KEY_ID` + `ASC_ISSUER_ID` com uma API Key **App Manager** válida.

## Parar de pedir permissão no Cursor

Cursor Settings → **Agents** → ative **Auto-run** / modo **Yolo** (ou “Run everything without asking”) para este workspace.  
Os prompts atuais vêm do Auto-review de segurança (keystore/API/upload), não do jogo em si.
