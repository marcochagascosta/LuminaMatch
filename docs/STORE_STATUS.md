# Lumina Match — Store Status

**Branch:** `feat/competitive-store`  
**Updated:** 2026-07-18  
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
- Chrome (sessão logada) estava em **Preparar versão** (`…/tracks/4701303588687654607/releases/2/prepare`) após Marco dizer **"subi"** (upload AAB).
- **Publicação / rollout automatizado: NÃO concluído** nesta sessão.
- Verificação UI do draft (AAB anexado + notas + Next/Review/Rollout) **não confirmada** por automação.

### Blockers de automação (Play)

1. **Browser MCP (cursor-ide-browser):** tabs criadas somem; `browser_navigate` falha com “No browser tab available” / “Browser view not found”.
2. **Chrome AppleScript JS:** desativado (`Ver > Desenvolvedor > Permitir o JavaScript do Eventos da Apple`). Sem isso não dá para ler/clicar na página do Play via `osascript`.
3. **Android Publisher API:** `gcloud` (`marko.formiga@gmail.com`) sem scope `androidpublisher` → 403 `ACCESS_TOKEN_SCOPE_INSUFFICIENT` em edits/IAP.
4. Sandbox do agente sem acesso ao perfil/cookies do Chrome (CDP/profile copy bloqueados).

### Helper no Desktop

- `Desktop/PUBLICAR_INTERNAL_PLAY.command` — após habilitar JS do Apple Events no Chrome, tenta preencher notas pt-BR e avançar Próximo/Revisar/Publicar no draft atual.

### Links úteis

- Internal testing: https://play.google.com/console/u/0/developers/6604076546202815303/app/4972110585725182702/tracks/internal-testing
- Preparar versão (draft): https://play.google.com/console/u/0/developers/6604076546202815303/app/4972110585725182702/tracks/4701303588687654607/releases/2/prepare
- Produtos únicos (IAP): https://play.google.com/console/u/0/developers/6604076546202815303/app/4972110585725182702/one-time-products
- Monetização: https://play.google.com/console/u/0/developers/6604076546202815303/app/4972110585725182702/monetization-setup

## Play IAP

- **Não criados** nesta sessão (mesmos blockers de UI/API).
- SKUs alvo = `docs/STORE_IAP_SKUS.md` (mesmos IDs da Apple).
- Conta merchant / pagamentos: **não verificada** — se a página de monetização pedir setup de merchant, é blocker adicional para preços BRL.

## Unity Ads

- **Pendente.** Sessão Unity Dashboard não autenticada (aba em login). Game IDs não configurados.

## Não enviar iOS deste Mac

macOS 27.0 beta → **ITMS-90111**. Archive App Store só em macOS estável.

## Ainda pendente (ação humana / próxima sessão)

1. Chrome: ativar **Permitir o JavaScript do Eventos da Apple** (ou usar browser MCP estável) → confirmar AAB 0.1.2 (3) no draft → notas → **Iniciar implantação** no teste interno.
2. Alternativa: rodar `Desktop/PUBLICAR_INTERNAL_PLAY.command` com JS Apple Events ligado.
3. Criar 7 Play IAPs (IDs iguais à Apple) + merchant account se pedido.
4. Unity Ads Game IDs.
5. iOS em Mac estável.

## Privacy

`Assets/Plugins/iOS/PrivacyInfo.xcprivacy` presente.

## Git note

Não commitar: `Assets/Scenes/Boot.unity`, `Assets/MobileDependencyResolver/**`, `ProjectSettings/GvhProjectSettings.xml` (ruído/resolver; sem segredos, mas fora do escopo store docs).
