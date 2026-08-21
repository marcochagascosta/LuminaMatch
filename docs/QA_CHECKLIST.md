# QA checklist — pronto para teste (0.1.36)

## Device (Galaxy / sideload)

- [ ] Home: Jogar tamanho normal; vidas/moedas ok
- [ ] HUD do jogo: countdown `+1 em MM:SS` no chip de vidas
- [ ] Níveis 1–3: tutorial básico
- [ ] Níveis 4–7: hints foguete / bomba / disco / combo
- [ ] Trocar dois poderes → combo (flash + SFX)
- [ ] Nível 8+: gelo quebra com match ao lado
- [ ] Nível 17+: caixas; poderes quebram caixas na linha
- [ ] Objetivo obstáculos mostra ícone de caixa
- [ ] Sem movimentos → Continuar (moedas/vídeo) / Tentar de novo
- [ ] Tempo esgotado → tela Result “Tempo esgotado” (não reinicia sozinho); Continuar dá +5 moves e +1:00
- [ ] Vitória → reveal do palácio; a cada 3 wins “Nova peça”
- [ ] Config: Sons / Música / Vibração
- [ ] Sem vidas → regen countdown + vídeo/loja
- [ ] 10 níveis seguidos sem crash

## Editor

- [ ] Edit Mode tests verdes (`Lumina Match/Run Edit Mode Tests Now`)
- [ ] Project abre sem erros de compile

## Builds locais

- APK: `Builds/Android/LuminaMatch-release.apk` / Desktop `LuminaMatch-0.1.36.apk`
- AAB: `Builds/Android/LuminaMatch-release.aab` / Desktop `LuminaMatch-0.1.36.aab`
- versionName `0.1.36` · versionCode `44`
- Home: barra do palácio + “próxima peça”
- Loja: scroll + sem IAP duplicado
- Níveis 31–40 curados (6 cores)

## Fora do escopo deste playtest (ação humana)

- AdMob unit IDs reais (hoje sample do Google)
- Google Play merchant + IAP
- Upload Internal Testing / iOS
