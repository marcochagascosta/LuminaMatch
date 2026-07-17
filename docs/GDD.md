# Lumina Match — GDD (produto competitivo de loja)

## O que é

Match-3 free-to-play estilo **Royal Match**: combine gemas para completar objetivos de nível e **restaurar o Palácio de Luz**.

## Fantasia

O **Palácio de Luz** (noite, cristal, ouro) desperta peça a peça conforme você vence níveis. O match-3 é o trabalho; o palácio é a recompensa emocional.

## Core loop

1. Gastar 1 vida → pré-nível (objetivos) → partida
2. Completar objetivos no limite de movimentos
3. Vitória → moedas + progresso do mapa + peça do palácio (1 a cada 3 vitórias)
4. Derrota → continue (moedas / rewarded / IAP)

## Board

- 8×8, gemas 2D facetadas (não quadrados de cor)
- Moldura, células, fundo noturno
- Bloqueios: gelo e caixa
- Power-ups no tabuleiro: match 4 = foguete, match 5 = bomba, L/T = disco de cor
- Boosters de inventário: martelo, troca, linha

## Conteúdo

- 60 níveis (`LevelCatalog` + overrides 1–10 / marcos)
- Tutorial guiado níveis 1–3
- Meta: 20 peças / stages visuais do palácio

## Economia / monetização (pressão moderada)

- Max 5 vidas, regen 20 min
- Continue: 900 moedas / +5 moves
- IAP: moedas P/M/G, vidas, boosters, remove ads, pacote estreia
- Ads: rewarded (continue/vida); interstitial limitado (180s, sem tutorial)
- Live ops leve: oferta diária + pacote de estreia

## Arte

- 2D IA (gemas/UI/board) + stages do palácio (`Resources/Art/`)
- Meshy opcional para 3D futuro (`docs/art/PALACE_MESHY_PROMPTS.md`)

## Fora de escopo (v1)

Battle pass, social, cloud save, 200+ níveis handcrafted, artista humano
