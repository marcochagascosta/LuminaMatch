# Lumina Match — Store Status

**Branch:** `feat/competitive-store`  
**Updated:** 2026-07-18 (sessão 2 — automação esgotada)  
**Marketing version:** `0.1.2` · Android versionCode `3` · iOS build `4`

## Builds (ready locally)

| Artefato | Path | Status |
|----------|------|--------|
| Android AAB | Desktop `LuminaMatch-0.1.2.aab` (= `Builds/Android/LuminaMatch-release.aab`) | Confirmado via bundletool: `package=com.marcosaas.luminamatch`, `versionName=0.1.2`, `versionCode=3` |
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

## Play Console — Internal Testing (2026-07-18)

- App ID: `4972110585725182702` · Developer: `6604076546202815303`
- Chrome (sessão logada) em **Preparar versão** após Marco dizer **"subi"** (upload AAB).
- **Publicação / rollout: AINDA NÃO concluído** por automação nesta sessão (nem na anterior).
- Versão vista nas abas abertas: páginas `…/releases/2/prepare` (draft) — confirmação visual AAB 0.1.2/code 3 **não lida por JS** (Apple Events desligado).

### Blockers de automação (Play) — reconfirmados

1. **Browser MCP (cursor-ide-browser):** tabs somem entre `new` e `navigate` (“No browser tab available” / “Browser view not found”).
2. **Chrome AppleScript JS:** runtime ainda recusa execute javascript apesar de `defaults … AppleEventsAllowJavaScriptFromAppleEvents = 1`. Menu UI-scripting não grava o ✓ (provável TCC/Acessibilidade). Clique manual no menu ainda necessário.
3. **Android Publisher API:** `gcloud` user token sem scope `androidpublisher`. ADC login com scope foi **iniciado** (browser OAuth aberto) mas **não concluído** (sem `application_default_credentials.json`). Sem service account no projeto.
4. **Sandbox do agente:** relaunch Chrome com `--remote-debugging-port` falha (`SingletonLock: Operation not permitted`) — não dá para CDP no perfil logado a partir do agente.
5. Unity Ads: aba em `login.unity.com` — não autenticado.

### Helper no Desktop (ação humana, ~2 min)

**Arquivo principal (duplo-clique):**  
`Desktop/FAZER_ISTO_AGORA_LUMINA_PLAY.command`

- Abre OAuth Google com scope Play → publica Internal via API + tenta 7 IAPs.
- Se API falhar: abre Preparar versão + checklist de 3 cliques (notas já no clipboard).

Extras (opcionais):

- `Desktop/LUMINA_PLAY_CHECKLIST.html` — checklist visual
- `Desktop/PUBLICAR_PLAY_VIA_API.command` — só API
- `Desktop/FINALIZAR_PLAY_2MIN.command` / `PUBLICAR_INTERNAL_PLAY.command` — UI via Apple Events (precisa marcar menu JS)

### Links úteis

- Internal testing: https://play.google.com/console/u/0/developers/6604076546202815303/app/4972110585725182702/tracks/internal-testing
- Preparar versão (draft): https://play.google.com/console/u/0/developers/6604076546202815303/app/4972110585725182702/tracks/4701303588687654607/releases/2/prepare
- Produtos únicos (IAP): https://play.google.com/console/u/0/developers/6604076546202815303/app/4972110585725182702/one-time-products
- Monetização: https://play.google.com/console/u/0/developers/6604076546202815303/app/4972110585725182702/monetization-setup

## Play IAP

- **Não criados** (mesmos blockers UI/API).
- SKUs alvo = `docs/STORE_IAP_SKUS.md` (mesmos IDs da Apple).
- Conta merchant / pagamentos: **não verificada** — se monetização pedir setup, é blocker para preços BRL.

## Unity Ads

- **Pendente.** Sessão Unity Dashboard não autenticada.

## Não enviar iOS deste Mac

macOS 27.0 beta → **ITMS-90111**. Archive App Store só em macOS estável.

## Ainda pendente (ação humana)

1. Duplo-clique `Desktop/FAZER_ISTO_AGORA_LUMINA_PLAY.command` → Permitir no Google **ou** 3 cliques manuais no draft.
2. Confirmar Internal Testing com 0.1.2.
3. Criar 7 Play IAPs (+ merchant se pedido).
4. Unity Ads Game IDs.
5. iOS em Mac estável.

## Privacy

`Assets/Plugins/iOS/PrivacyInfo.xcprivacy` presente.

## Git note

Não commitar: `Assets/Scenes/Boot.unity`, `Assets/MobileDependencyResolver/**`, `ProjectSettings/GvhProjectSettings.xml` (ruído/resolver; sem segredos, mas fora do escopo store docs).
