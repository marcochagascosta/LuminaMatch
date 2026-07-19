# Lumina Match — Store Status

**Branch:** `feat/competitive-store`  
**Updated:** 2026-07-17  
**Marketing version:** `0.1.2` · Android versionCode `3` · iOS build `4`

## Builds (ready locally)

| Artefato | Path | Status |
|----------|------|--------|
| Android AAB | `Builds/Android/LuminaMatch-release.aab` + Desktop `LuminaMatch-0.1.2.aab` | Pronto p/ Play internal |
| iOS IPA | Desktop `LuminaMatch-build4.ipa` (0.1.2 / 4, ~96 MB) | Archive OK neste Mac |
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

## Play Console

- App ID: `4972110585725182702` · Developer: `6604076546202815303`
- Teste interno ativo com `0.1.1` (2); rascunho de nova versão aberto em Preparar versão
- Upload AAB automatizado bloqueado (sistema não permite anexar arquivo via CDP) — precisa 1 clique humano no seletor de arquivo

## Não enviar iOS deste Mac

macOS 27.0 beta (`26A5378n`) → **ITMS-90111**. Archive App Store só em macOS estável.

## Ainda pendente

1. Marco: no diálogo do Play, escolher `Desktop/LuminaMatch-0.1.2.aab` → eu avanço o resto
2. Play IAP + merchant account
3. Unity Ads Game IDs
4. iOS em Mac estável

## Privacy

`Assets/Plugins/iOS/PrivacyInfo.xcprivacy` presente.
