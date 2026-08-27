# QA checklist — pronto para teste (0.1.41)

## Device (Galaxy / Play Internal / sideload release)

- [ ] Home: Jogar; vidas/moedas; palácio
- [ ] HUD: countdown `+1 em MM:SS` no chip de vidas
- [ ] Níveis 1–3: tutorial
- [ ] Níveis 4–7: hints foguete / bomba / disco / combo
- [ ] Trocar dois poderes → combo
- [ ] Nível 8+: gelo; 17+: caixas
- [ ] Sem movimentos → Continuar (moedas/vídeo) / Tentar de novo
- [ ] Tempo esgotado → Result; Continuar +5 moves
- [ ] Vitória → reveal do palácio
- [ ] Config: Sons / Música / Vibração (ícones)
- [ ] Sem vidas → regen + vídeo/loja
- [ ] Loja: preços da loja (não sandbox) em build **Release**
- [ ] Restore compras
- [ ] 10 níveis seguidos sem crash

## Editor

- [ ] Edit Mode tests verdes
- [ ] Project abre sem erros de compile

## Builds

- AAB release: `Builds/Android/LuminaMatch-release.aab` · **0.1.41 / 49**
- APK release: sideload OK; **não** enviar Debug APK às lojas
- iOS: `Builds/iOS/` → Archive build **50** em macOS estável

## Store gate

- [ ] Play Internal/Closed OK no device
- [ ] Play Production promote (após merchant + QA)
- [ ] TestFlight / ASC Review com build 50
- [ ] AdMob fills (ou graceful fail sem grant grátis)
- [ ] IAP sandbox Apple / license testers Play
