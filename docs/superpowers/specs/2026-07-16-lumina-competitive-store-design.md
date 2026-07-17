# Lumina Match — Design: produto competitivo de loja

**Data:** 2026-07-16  
**Status:** aprovado em brainstorming (Marco)  
**Projeto:** `/Users/marcocosta/Games/LuminaMatch`  
**Abordagem:** evoluir o projeto atual (não reescrever do zero)

---

## O que é o jogo (explicação)

**Lumina Match** é um jogo mobile de **match-3** free-to-play no estilo **Royal Match**.

Você joga em um tabuleiro 8×8 cheio de **gemas**. O movimento básico é trocar duas gemas vizinhas para formar grupos de 3 ou mais da mesma cor. Cada nível tem um **objetivo** (por exemplo: coletar N gemas vermelhas, limpar gelo/caixas, ou atingir um score) e um **limite de movimentos**. Gastar uma **vida** entra no nível; ao vencer, ganha moedas e avança no mapa.

O gancho de longo prazo não é só “passar de fase”: cada vitória ajuda a **restaurar o Palácio de Luz** — um castelo noturno de cristal que ganha peças e salas conforme você progride. O jogador deve sentir, em menos de um minuto: *eu combino gemas → completo o nível → o palácio fica mais bonito*.

Para tornar as partidas mais interessantes e competitivas, matches maiores criam **power-ups no tabuleiro** (foguete, bomba, disco de cor). Há também **boosters** de inventário (martelo, troca, linha). Sem vidas ou movimentos, o jogador pode esperar, assistir anúncio recompensado, gastar moedas ou comprar packs (IAP) — com pressão moderada, típica de casual top-grossing.

Em resumo: **match-3 premium + meta de construção do palácio + monetização free-to-play**, com visual elaborado (gemas de verdade, board com moldura, palácio via Meshy), pronto para as lojas.

---

## 1. Objetivo

Transformar o MVP técnico atual (match-3 jogável, UI procedural, monetização sandbox) em um **produto free-to-play competitivo** no estilo **Royal Match**, atrativo o suficiente para o jogador **entender o objetivo**, **querer continuar** e **gastar dinheiro** de forma sustentável.

### Critério de sucesso (v1 cobrando)

- Em 30–60 segundos fica claro: *combine gemas → complete o nível → restaure o Palácio de Luz*
- Board e UI têm cara de jogo casual premium (não protótipo de cores sólidas)
- Power-ups no tabuleiro (4+/5+/L-T) e boosters de inventário usáveis
- Palácio progressivo visível (pipeline Meshy)
- IAP e ads **reais** ligados; oferta de estreia + oferta diária
- Monetização com **pressão moderada** (tom Royal Match), não paywall no tutorial
- Builds iOS/Android submetíveis; archive iOS em **macOS estável** (evitar ITMS-90111)

---

## 2. Decisões de produto (fechadas)

| Tema | Escolha |
|------|---------|
| Escopo | Produto completo de loja (visual + power-ups + IAP/ads + live ops leve) |
| Referência | Royal Match |
| Arte | 2D gerada (IA) + Meshy no palácio; **sem** contratação de artista |
| Monetização | Pressão moderada (vidas/continue/ofertas); free-to-play realista |
| Código | Evoluir codebase atual |

---

## 3. Fantasia e jornada

**Fantasia:** restaurar o **Palácio de Luz** (noite, cristal, ouro suave). Match-3 é o trabalho; o palácio é a recompensa emocional.

**Minuto 0–2:**

1. Home com palácio incompleto + **Jogar**
2. Tutorial: combine 3 → objetivo do nível 1
3. Vitória → peça/área do palácio aparece (feedback visual imediato)

**Sessão:** vidas → pré-nível → partida → vitória/derrota/continue → mapa/palácio.

**Longo prazo:** mapa linear, progresso do palácio, power-ups, loja, oferta de estreia, oferta diária.

---

## 4. Gameplay e board (requisito visual Royal Match)

### Board 8×8

- Joias **2D com formato de gema** (facetado, brilho, sombra) — **não** quadrados/bolinhas só de cor
- Moldura do tabuleiro, células com relevo, fundo de cena
- Bloqueios gelo/caixa com arte legível sobre a gema
- Seleção, hint e combos com VFX claros
- Juice: swap animado, pop, cascata, feedback de combo

### Objetivos

- HUD com ícone + quantidade restante
- Card de pré-nível: “o que fazer” antes de gastar vida
- Tipos: coletar cor, limpar bloqueios, score (manter e melhorar apresentação)

### Power-ups no tabuleiro (novo)

- Match 4 → foguete (linha/coluna)
- Match 5 → bomba (área)
- Match L/T → **disco de cor** (limpa todas as gemas daquela cor); se L/T for complexo demais na v1, degradar para bomba — mas o mapping oficial do design é disco de cor
- Combos power + power
- Boosters de inventário existentes (martelo, troca, linha) com ícones e UX clara

### Conteúdo

- ~60 níveis no lançamento
- Curva melhorada: procedural + **overrides** em níveis-chave (1–10 e marcos do palácio)
- Níveis 1–3 = tutorial guiado

---

## 5. Meta do palácio e telas

### Palácio (Meshy)

- Home mostra progresso visual (etapas antes/depois)
- Progressão por vitórias libera peças/áreas **visíveis** (não só texto)
- Tela Palácio lista etapas desbloqueadas e pendentes
- Pipeline: prompts Meshy → import (FBX/GLB e/ou renders 2D) → stages em `CastleProgress`

### Telas

Home · Mapa · Pré-nível · Partida · Vitória/Derrota/Sem vidas · Loja · Palácio · Ofertas (estreia/diária)

UI premium (prefabs), não wireframe gerado só com `Image`/`Text` coloridos.

---

## 6. Economia, IAP, ads, live ops

### Economia

- Max 5 vidas; regen **20 min** (igual ao MVP; calibrar depois se necessário)
- Continue: moedas **ou** rewarded **ou** IAP
- Soft currency + boosters
- Recompensas free suficientes para progressão; continue/boosters incentivam gasto

### IAP (Unity IAP — produção)

- Moedas P/M/G
- Refil de vidas
- Pack de boosters
- Remove ads
- Pacote de estreia (early game, alto valor percebido)

### Ads (padrão: **Unity Ads**; AdMob só se Unity Ads bloquear publicação)

- Rewarded: vida, continue, ocasionalmente booster
- Interstitial: frequência limitada (ex. após derrota / a cada N níveis); respeita Remove Ads

### Live ops mínimo (v1)

- Oferta diária (pack rotativo, timer 24h, local)
- Pacote de estreia (1–2 exposições no early game)
- Fora de escopo v1: battle pass, clãs, servidor de eventos

### Proibido

- Paywall no tutorial
- Ads em alta frequência (spam)

---

## 7. Arquitetura técnica

### Manter

- `BoardModel`, `MatchFinder`, `Match3Session`, `PlayerProgress`, `LevelCatalog`
- `IIapService` / `IAdsService` + `MonetizationHub`
- Testes EditMode existentes (estender)

### Refatorar

- `UiRoot` monolítico → telas/prefabs + presenter do board (sprites/VFX separados da lógica)

### Estender

- Power-ups no core de match
- Overrides de nível
- Implementações reais de IAP/ads (sandbox só Editor/dev)
- Ofertas locais (estreia + diária)
- Stages visuais do palácio (Meshy)

### Arte (pastas)

- `Assets/Art/` — joias, UI, blockers, VFX
- `Assets/Art/Palace/` — assets Meshy / renders

### Qualidade e store

- Testes EditMode: match, power-ups, economia, ofertas
- Builds iOS/Android
- **Archive App Store em macOS estável** (ITMS-90111 se buildado em macOS 27 beta)

---

## 8. Fatias de entrega

Ordem sugerida (produto C, mas entregável incrementalmente):

1. **Visual + tutorial + board Royal Match** (objetivo óbvio + juice)
2. **Power-ups + palácio Meshy**
3. **IAP/ads reais + oferta estreia/diária**
4. **Polish, calibração econômica, submit lojas** (iOS em host estável)

---

## 9. Fora de escopo (v1)

- Artista humano contratado
- Battle pass / social / cloud save
- 200+ níveis handcrafted
- Paridade AAA com Playrix/Dream Games em volume de conteúdo
- Servidor de live ops

---

## 10. Dependências / riscos

| Risco | Mitigação |
|-------|-----------|
| Qualidade de arte IA/Meshy inconsistente | Direção visual fixa (noite/cristal/ouro); lista de assets; iterar prompts; preferir renders 2D do Meshy se 3D for pesado |
| `UiRoot` difícil de fatiar | Extrair telas uma a uma sem big-bang |
| ITMS-90111 (macOS beta) | Archive em macOS 26.x estável ou Xcode Cloud |
| SDKs IAP/ads | Manter interfaces; feature flags; sandbox em dev |
| Escopo C amplo | Respeitar fatias; não bloquear fatia 1 por fatia 3 |

---

## 11. Aprovação

- Fantasia/jornada: aprovada  
- Gameplay/board (+ board elaborado tipo Royal Match): aprovada  
- Palácio/telas: aprovada  
- Monetização: aprovada  
- Arquitetura: aprovada  
- Abordagem de execução: evoluir projeto atual  
