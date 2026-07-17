# Lumina Match Competitive Store Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Evoluir o MVP técnico do Lumina Match para um free-to-play competitivo estilo Royal Match (board premium, tutorial, power-ups, palácio Meshy, IAP/ads reais, oferta estreia/diária) pronto para cobrar nas lojas.

**Architecture:** Manter o core em `Assets/Scripts/Match3/` e economia em `PlayerProgress`; fatiar `UiRoot` em telas/prefabs; separar apresentação do board (`BoardPresenter`) da lógica; estender `MatchFinder`/`BoardModel` com power-ups; plugar implementações reais atrás de `IIapService`/`IAdsService`; palácio via stages + assets Meshy.

**Tech Stack:** Unity 6000.5.x, C#, NUnit EditMode, Unity Purchasing, Unity Ads (fallback AdMob), sprites 2D (IA), Meshy (3D/renders), iOS/Android.

**Spec:** `docs/superpowers/specs/2026-07-16-lumina-competitive-store-design.md`

## Global Constraints

- Referência visual/feel: **Royal Match** (gemas facetadas, board com moldura, UI premium).
- Arte: **2D IA + Meshy no palácio**; sem artista humano.
- Monetização: pressão **moderada**; sem paywall no tutorial; ads sem spam.
- Ads padrão: **Unity Ads**; AdMob só se necessário.
- Vidas: max **5**, regen **20 min**; continue **900** moedas (calibrar depois).
- Power-ups: match4=foguete, match5=bomba, L/T=disco de cor.
- Archive iOS App Store em **macOS estável** (não macOS 27 beta) — evita ITMS-90111.
- Idioma UI/copy: **pt-BR**.
- Commits frequentes por task; TDD no core de match/economia.

## File map (alvo)

| Path | Responsabilidade |
|------|------------------|
| `Assets/Scripts/Match3/Match3Types.cs` | Enums: `BoardPowerType`, campos em `Cell` |
| `Assets/Scripts/Match3/MatchFinder.cs` | Matches + classificação 4/5/L/T |
| `Assets/Scripts/Match3/PowerUpResolver.cs` | Spawn e ativação de foguete/bomba/disco |
| `Assets/Scripts/Match3/BoardModel.cs` | Integra resolve com power-ups |
| `Assets/Scripts/Match3/LevelCatalog.cs` | Overrides níveis 1–10 / marcos |
| `Assets/Scripts/Match3/Match3Session.cs` | Sessão inalterada na API; usa board estendido |
| `Assets/Scripts/UI/BoardPresenter.cs` | Render sprites, juice, input |
| `Assets/Scripts/UI/Screens/*.cs` | Home, Map, PreLevel, BoardHud, Results, Shop, Palace, Tutorial |
| `Assets/Scripts/UI/UiRoot.cs` | Orquestra telas (encolhe ao longo das tasks) |
| `Assets/Scripts/UI/TutorialDirector.cs` | FTUE níveis 1–3 |
| `Assets/Scripts/Meta/CastleProgress.cs` | Stages visuais + nomes |
| `Assets/Scripts/Meta/PalaceStageVisual.cs` | Liga stage → prefab/sprite Meshy |
| `Assets/Scripts/Economy/PlayerProgress.cs` | Flags tutorial/ofertas |
| `Assets/Scripts/Economy/OfferService.cs` | Pacote estreia + oferta diária |
| `Assets/Scripts/Monetization/UnityIapService.cs` | IAP produção |
| `Assets/Scripts/Monetization/UnityAdsService.cs` | Ads produção |
| `Assets/Scripts/Monetization/MonetizationHub.cs` | Escolhe sandbox vs real |
| `Assets/Art/Gems/`, `Board/`, `UI/`, `Blockers/`, `Vfx/` | Sprites 2D |
| `Assets/Art/Palace/` | Meshy imports / renders |
| `Assets/Tests/EditMode/*` | Testes NUnit |

---

## Slice 1 — Visual Royal Match + tutorial + objetivos claros

### Task 1: Tipos de power no `Cell` (preparação, sem gameplay ainda)

**Files:**
- Modify: `Assets/Scripts/Match3/Match3Types.cs`
- Modify: `Assets/Tests/EditMode/Match3Tests.cs`
- Test: EditMode

**Interfaces:**
- Consumes: `Cell`, `GemColor` existentes
- Produces: `enum BoardPowerType { None, Rocket, Bomb, ColorDisk }` e `Cell.Power`

- [ ] **Step 1: Write the failing test**

```csharp
[Test]
public void Cell_DefaultPower_IsNone()
{
    var c = new Cell { Color = GemColor.Ruby };
    Assert.AreEqual(BoardPowerType.None, c.Power);
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: abrir Unity → EditMode → `Cell_DefaultPower_IsNone`  
Expected: FAIL (tipo/`Power` inexistente)

- [ ] **Step 3: Minimal implementation**

Em `Match3Types.cs`, adicionar:

```csharp
public enum BoardPowerType
{
    None = 0,
    Rocket = 1,
    Bomb = 2,
    ColorDisk = 3
}
```

Em `struct Cell`, adicionar:

```csharp
public BoardPowerType Power;
```

- [ ] **Step 4: Run test to verify it passes**

Expected: PASS

- [ ] **Step 5: Commit**

```bash
git add Assets/Scripts/Match3/Match3Types.cs Assets/Tests/EditMode/Match3Tests.cs
git commit -m "$(cat <<'EOF'
feat(match3): add BoardPowerType on Cell for upcoming power-ups

EOF
)"
```

---

### Task 2: Pipeline de arte 2D das gemas (Royal Match)

**Files:**
- Create: `Assets/Art/Gems/gem_crystal.png` … `gem_amethyst.png` (6 cores)
- Create: `Assets/Art/Gems/GemSprites.cs` (ScriptableObject map cor→sprite)
- Create: `docs/art/GEM_PROMPTS.md` (prompts IA reutilizáveis)
- Modify: nenhum gameplay ainda

**Interfaces:**
- Produces: `GemSprites.Get(GemColor) → Sprite`

- [ ] **Step 1: Document prompts**

Criar `docs/art/GEM_PROMPTS.md` com prompt base:

```text
Mobile match-3 gem icon, Royal Match style, faceted crystal [COLOR],
soft specular highlight, slight drop shadow, centered, opaque,
square 256x256 PNG, transparent background, no text, no UI chrome
```

Cores: Crystal=branco-azul gelo, Amber=âmbar, Sapphire=azul, Emerald=verde, Ruby=vermelho, Amethyst=roxo.

- [ ] **Step 2: Generate and import sprites**

Gerar 6 PNGs 256×256, import Unity: Texture Type=Sprite (2D), Pixels Per Unit=100, Filter=Bilinear, Compression=High Quality.

- [ ] **Step 3: ScriptableObject**

```csharp
using UnityEngine;
using LuminaMatch.Match3;

[CreateAssetMenu(menuName = "Lumina/Gem Sprites")]
public class GemSprites : ScriptableObject
{
    public Sprite Crystal, Amber, Sapphire, Emerald, Ruby, Amethyst;

    public Sprite Get(GemColor c) => c switch
    {
        GemColor.Crystal => Crystal,
        GemColor.Amber => Amber,
        GemColor.Sapphire => Sapphire,
        GemColor.Emerald => Emerald,
        GemColor.Ruby => Ruby,
        GemColor.Amethyst => Amethyst,
        _ => null
    };
}
```

Criar asset `Assets/Art/Gems/GemSprites.asset` e arrastar sprites.

- [ ] **Step 4: Visual check in Editor**

Abrir asset; confirmar 6 slots preenchidos.

- [ ] **Step 5: Commit**

```bash
git add Assets/Art/Gems docs/art/GEM_PROMPTS.md
git commit -m "$(cat <<'EOF'
feat(art): add Royal Match-style gem sprites and GemSprites map

EOF
)"
```

---

### Task 3: Arte do board (moldura, célula, blockers)

**Files:**
- Create: `Assets/Art/Board/board_frame.png`, `cell.png`, `bg_night.png`
- Create: `Assets/Art/Blockers/ice.png`, `box.png`
- Create: `Assets/Art/Board/BoardTheme.cs` (SO)

- [ ] **Step 1: Generate assets**

Prompts em `docs/art/BOARD_PROMPTS.md`:
- Frame ornamento ouro/cristal noite
- Célula madeira escura com relevo
- Fundo palácio noturno blur
- Gelo translúcido overlay
- Caixa madeira fechada overlay

- [ ] **Step 2: BoardTheme SO**

```csharp
using UnityEngine;

[CreateAssetMenu(menuName = "Lumina/Board Theme")]
public class BoardTheme : ScriptableObject
{
    public Sprite Frame;
    public Sprite Cell;
    public Sprite Background;
    public Sprite Ice;
    public Sprite Box;
}
```

- [ ] **Step 3: Commit**

```bash
git add Assets/Art/Board Assets/Art/Blockers docs/art/BOARD_PROMPTS.md
git commit -m "$(cat <<'EOF'
feat(art): add board frame, cells, blockers, night background

EOF
)"
```

---

### Task 4: `BoardPresenter` — render por sprites (substituir quadrados)

**Files:**
- Create: `Assets/Scripts/UI/BoardPresenter.cs`
- Modify: `Assets/Scripts/UI/UiRoot.cs` (usar presenter no board)
- Prefab opcional: `Assets/Prefabs/UI/BoardCell.prefab`

**Interfaces:**
- Consumes: `BoardModel`, `GemSprites`, `BoardTheme`
- Produces: `void Bind(BoardModel board)`, `void Refresh()`, `event Action<int,int> CellClicked`

- [ ] **Step 1: Implement BoardPresenter**

```csharp
using System;
using UnityEngine;
using UnityEngine.UI;
using LuminaMatch.Match3;

namespace LuminaMatch.UI
{
    public class BoardPresenter : MonoBehaviour
    {
        public GemSprites Gems;
        public BoardTheme Theme;
        public RectTransform GridRoot;
        public Image Background;
        public Image Frame;

        public event Action<int, int> CellClicked;

        BoardModel _board;
        Image[,] _cells;

        public void Bind(BoardModel board)
        {
            _board = board;
            // build w*h child Images under GridRoot using Theme.Cell
            // wire Button/click → CellClicked(x,y)
            Refresh();
        }

        public void Refresh()
        {
            for (int y = 0; y < _board.Height; y++)
            for (int x = 0; x < _board.Width; x++)
            {
                var cell = _board.Grid[x, y];
                var img = _cells[x, y];
                img.sprite = cell.Blocker == BlockerType.Box
                    ? Theme.Box
                    : Gems.Get(cell.Color);
                // overlay ice if BlockerType.Ice via child Image
            }
        }
    }
}
```

(Implementação completa no código: criar grid, overlays gelo, highlight seleção.)

- [ ] **Step 2: Wire UiRoot**

Substituir `BuildBoard`/`GemColorToUi` por `BoardPresenter.Bind(session.Board)` e clicks → `TrySwap` existente.

- [ ] **Step 3: Play Mode smoke**

Boot → Jogar → ver gemas sprite + moldura; swap ainda resolve matches.

- [ ] **Step 4: Commit**

```bash
git add Assets/Scripts/UI/BoardPresenter.cs Assets/Scripts/UI/UiRoot.cs Assets/Prefabs/UI
git commit -m "$(cat <<'EOF'
feat(ui): render match board with gem sprites and frame theme

EOF
)"
```

---

### Task 5: HUD de objetivos com ícones + pré-nível

**Files:**
- Create: `Assets/Scripts/UI/Screens/PreLevelScreen.cs`
- Create: `Assets/Scripts/UI/ObjectiveHud.cs`
- Create: `Assets/Art/UI/obj_collect.png`, `obj_blocker.png`, `obj_score.png`
- Modify: `Assets/Scripts/UI/UiRoot.cs`

- [ ] **Step 1: ObjectiveHud**

Mostrar lista: ícone + texto curto + restante (`Match3Session` já expõe progresso via métodos existentes / estender se preciso).

- [ ] **Step 2: PreLevelScreen**

Antes de `TrySpendLife`: card “Nível N”, objetivos, botões boosters, **Jogar**.

- [ ] **Step 3: Play Mode**

Entrar nível 5+ e confirmar objetivos legíveis sem ler só uma string crua.

- [ ] **Step 4: Commit**

```bash
git add Assets/Scripts/UI/Screens/PreLevelScreen.cs Assets/Scripts/UI/ObjectiveHud.cs Assets/Art/UI Assets/Scripts/UI/UiRoot.cs
git commit -m "$(cat <<'EOF'
feat(ui): pre-level card and icon-based objective HUD

EOF
)"
```

---

### Task 6: Tutorial FTUE (níveis 1–3)

**Files:**
- Create: `Assets/Scripts/UI/TutorialDirector.cs`
- Modify: `Assets/Scripts/Economy/PlayerProgress.cs` (`PlayerSaveData.TutorialStep`)
- Modify: `Assets/Scripts/Match3/LevelCatalog.cs` (nível 1 bem fácil)

**Interfaces:**
- Produces: `TutorialDirector.MaybeShow(levelId)`, avança `TutorialStep` 0→3

- [ ] **Step 1: Save field**

```csharp
// PlayerSaveData
public int TutorialStep; // 0=not started, 1=swap done, 2=objective explained, 3=done
```

- [ ] **Step 2: TutorialDirector overlays**

Nível 1: hand/hint “Troque duas gemas para fazer 3 iguais”.  
Nível 2: “Complete o objetivo no topo”.  
Nível 3: “Vitórias restauram o Palácio de Luz”.  
Sem bloquear compras; sem ads.

- [ ] **Step 3: Soften level 1 in LevelCatalog**

Moves altos, 4 cores, sem blockers, objetivo CollectColor baixo.

- [ ] **Step 4: Play Mode fresh save**

Apagar PlayerPrefs chave `LuminaMatch.Save.v1` → fluxo tutorial completo.

- [ ] **Step 5: Commit**

```bash
git add Assets/Scripts/UI/TutorialDirector.cs Assets/Scripts/Economy/PlayerProgress.cs Assets/Scripts/Match3/LevelCatalog.cs
git commit -m "$(cat <<'EOF'
feat(ftue): guided tutorial for levels 1-3 and softer opener

EOF
)"
```

---

### Task 7: Juice mínimo (swap / pop / cascata)

**Files:**
- Modify: `Assets/Scripts/UI/BoardPresenter.cs`
- Create: `Assets/Scripts/UI/BoardJuice.cs` (coroutines DOTween opcional; se sem DOTween, `LeanTween` ou `Coroutine` + `Vector3.Lerp`)

- [ ] **Step 1: Animate swap** (0.12s lerp posições)
- [ ] **Step 2: Scale-down pop on clear**
- [ ] **Step 3: Stagger gravity visually after `ApplyGravity`**
- [ ] **Step 4: Commit**

```bash
git add Assets/Scripts/UI/BoardPresenter.cs Assets/Scripts/UI/BoardJuice.cs
git commit -m "$(cat <<'EOF'
feat(ui): add swap, pop, and cascade juice on the board

EOF
)"
```

---

## Slice 2 — Power-ups no board + palácio Meshy

### Task 8: Classificar runs (4 / 5 / L-T) — TDD

**Files:**
- Create: `Assets/Scripts/Match3/MatchShapeAnalyzer.cs`
- Modify: `Assets/Tests/EditMode/Match3Tests.cs`

**Interfaces:**
- Produces: `MatchShapeAnalyzer.Analyze(Cell[,] grid, HashSet<(int x,int y)> matched) → List<PowerSpawn>`  
  onde `PowerSpawn { int X,Y; BoardPowerType Type; GemColor Color }`

- [ ] **Step 1: Failing tests**

```csharp
[Test]
public void Analyze_HorizontalFour_SpawnsRocket()
{
    var grid = Empty(8, 8);
    for (int x = 0; x < 4; x++) grid[x, 0].Color = GemColor.Ruby;
    var matched = MatchFinder.FindMatches(grid);
    var spawns = MatchShapeAnalyzer.Analyze(grid, matched);
    Assert.AreEqual(1, spawns.Count);
    Assert.AreEqual(BoardPowerType.Rocket, spawns[0].Type);
}

[Test]
public void Analyze_HorizontalFive_SpawnsBomb()
{
    var grid = Empty(8, 8);
    for (int x = 0; x < 5; x++) grid[x, 0].Color = GemColor.Sapphire;
    var matched = MatchFinder.FindMatches(grid);
    var spawns = MatchShapeAnalyzer.Analyze(grid, matched);
    Assert.AreEqual(BoardPowerType.Bomb, spawns[0].Type);
}

[Test]
public void Analyze_LShape_SpawnsColorDisk()
{
    var grid = Empty(8, 8);
    // 3 horizontal + 2 vertical forming L (5 cells of Emerald)
    grid[0,0].Color = grid[1,0].Color = grid[2,0].Color = GemColor.Emerald;
    grid[0,1].Color = grid[0,2].Color = GemColor.Emerald;
    var matched = MatchFinder.FindMatches(grid);
    var spawns = MatchShapeAnalyzer.Analyze(grid, matched);
    Assert.IsTrue(spawns.Exists(s => s.Type == BoardPowerType.ColorDisk));
}
```

- [ ] **Step 2: Run — expect FAIL**
- [ ] **Step 3: Implement MatchShapeAnalyzer** (priorizar 5 > L/T > 4; centro do run = spawn)
- [ ] **Step 4: Run — expect PASS**
- [ ] **Step 5: Commit**

```bash
git add Assets/Scripts/Match3/MatchShapeAnalyzer.cs Assets/Tests/EditMode/Match3Tests.cs
git commit -m "$(cat <<'EOF'
feat(match3): classify match shapes into rocket, bomb, color disk

EOF
)"
```

---

### Task 9: `PowerUpResolver` + integração `BoardModel.ResolveMatches`

**Files:**
- Create: `Assets/Scripts/Match3/PowerUpResolver.cs`
- Modify: `Assets/Scripts/Match3/BoardModel.cs`
- Test: EditMode novos testes

**Interfaces:**
- `PowerUpResolver.ExpandActivation(grid, x, y) → HashSet<(int,int)>`  
- Rocket: limpa linha **ou** coluna (alternar por `x+y` par/ímpar)  
- Bomb: raio 1 (3×3)  
- ColorDisk: todas células com mesma `GemColor` (não holes)

- [ ] **Step 1: Tests for each power clear set**
- [ ] **Step 2: Implement resolver**
- [ ] **Step 3: After clearing a match, spawn power on designated cell (`Color=None` temporário ou manter cor + `Power!=None`); next resolve if player swaps power+power or matches including power, expand clears**
- [ ] **Step 4: Tests PASS**
- [ ] **Step 5: Commit**

```bash
git add Assets/Scripts/Match3/PowerUpResolver.cs Assets/Scripts/Match3/BoardModel.cs Assets/Tests/EditMode/Match3Tests.cs
git commit -m "$(cat <<'EOF'
feat(match3): spawn and detonate board power-ups during resolve

EOF
)"
```

---

### Task 10: Sprites/VFX de power-ups + presenter

**Files:**
- Create: `Assets/Art/Vfx/rocket.png`, `bomb.png`, `color_disk.png`
- Modify: `BoardPresenter` / `GemSprites` para mostrar `cell.Power`

- [ ] **Step 1: Art + map**
- [ ] **Step 2: Presenter shows power icon over gem**
- [ ] **Step 3: Play Mode create match-4 → rocket appears**
- [ ] **Step 4: Commit**

```bash
git add Assets/Art/Vfx Assets/Scripts/UI/BoardPresenter.cs Assets/Art/Gems
git commit -m "$(cat <<'EOF'
feat(ui): show rocket, bomb, and color-disk power sprites

EOF
)"
```

---

### Task 11: Overrides de níveis-chave

**Files:**
- Modify: `Assets/Scripts/Match3/LevelCatalog.cs`
- Test: `LevelCatalog` tests

- [ ] **Step 1: Add `TryGetOverride(int levelId, out LevelDefinition def)` table for 1–10 and every castle milestone (3,6,9,…)**
- [ ] **Step 2: `Get(id)` returns override if present else procedural**
- [ ] **Step 3: Tests for level 1 moves/colors**
- [ ] **Step 4: Commit**

```bash
git add Assets/Scripts/Match3/LevelCatalog.cs Assets/Tests/EditMode/Match3Tests.cs
git commit -m "$(cat <<'EOF'
feat(levels): hand-tune key levels via catalog overrides

EOF
)"
```

---

### Task 12: Pipeline Meshy + stages do palácio

**Files:**
- Create: `docs/art/PALACE_MESHY_PROMPTS.md`
- Create: `Assets/Art/Palace/` (FBX/GLB ou PNG stages 0–20)
- Create: `Assets/Scripts/Meta/PalaceStageVisual.cs`
- Modify: `Assets/Scripts/Meta/CastleProgress.cs`
- Modify: Home UI para mostrar stage atual

**Interfaces:**
- `CastleProgress.CurrentStageIndex(progress) → int` (0..TotalPieces)
- `PalaceStageVisual.Show(int piecesUnlocked)`

- [ ] **Step 1: Write Meshy prompts** (noite, cristal, ouro; 5–8 key stages mínimo se 20 full for caro — interpolar visualmente entre keyframes)

Prompt base:

```text
Night crystal palace exterior, soft gold trim, luminous windows,
fantasy mobile game hero prop, centered, clean silhouette,
game-ready low poly, PBR, no characters, no text
```

Stages: ruínas → portão → torres → cúpula → coroa.

- [ ] **Step 2: Import Meshy outputs** into `Assets/Art/Palace/stage_XX.*`
- [ ] **Step 3: PalaceStageVisual swaps mesh/sprite by `CastlePieces`**
- [ ] **Step 4: On win, play short reveal of new piece**
- [ ] **Step 5: Home + Palace screen**
- [ ] **Step 6: Commit**

```bash
git add docs/art/PALACE_MESHY_PROMPTS.md Assets/Art/Palace Assets/Scripts/Meta Assets/Scripts/UI
git commit -m "$(cat <<'EOF'
feat(meta): visible Palace of Light stages via Meshy assets

EOF
)"
```

---

## Slice 3 — IAP/ads reais + live ops leve

### Task 13: `OfferService` (estreia + diária) — TDD sem UnityPurchasing

**Files:**
- Create: `Assets/Scripts/Economy/OfferService.cs`
- Modify: `PlayerSaveData` (`StarterPackBought`, `DailyOfferDayKey`, `DailyOfferSku`)
- Create: `Assets/Tests/EditMode/OfferServiceTests.cs`

**Interfaces:**
- `bool ShouldShowStarterPack(PlayerProgress p)`
- `bool TryClaimDailyOffer(PlayerProgress p, DateTime utcNow, out IapProductId product)`
- Starter: show if `!StarterPackBought && LevelsWon <= 5 && TutorialStep >= 3`
- Daily: one claim per UTC day

- [ ] **Step 1: Failing tests for starter visibility and daily once/day**
- [ ] **Step 2: Implement OfferService**
- [ ] **Step 3: PASS**
- [ ] **Step 4: Commit**

```bash
git add Assets/Scripts/Economy/OfferService.cs Assets/Scripts/Economy/PlayerProgress.cs Assets/Tests/EditMode/OfferServiceTests.cs
git commit -m "$(cat <<'EOF'
feat(economy): starter pack and daily offer eligibility rules

EOF
)"
```

---

### Task 14: UI de ofertas + loja polish

**Files:**
- Create: `Assets/Scripts/UI/Screens/OfferPopup.cs`
- Modify: Shop screen / `UiRoot`

- [ ] **Step 1: Starter popup after tutorial win**
- [ ] **Step 2: Daily button on Home with countdown**
- [ ] **Step 3: Shop lists products with `GetPriceLabel`**
- [ ] **Step 4: Commit**

```bash
git add Assets/Scripts/UI/Screens/OfferPopup.cs Assets/Scripts/UI
git commit -m "$(cat <<'EOF'
feat(ui): starter and daily offer popups plus shop polish

EOF
)"
```

---

### Task 15: Unity IAP produção + produto StarterPack

**Files:**
- Create: `Assets/Scripts/Monetization/UnityIapService.cs`
- Modify: `IIapService.cs` / `IapProductId` add `StarterPack`
- Modify: `SandboxIapService` grant starter (coins+boosters+lives)
- Modify: `MonetizationHub.cs` `#if UNITY_EDITOR || LUMINA_SANDBOX` → sandbox else UnityIap
- Create: `docs/STORE_IAP_SKUS.md` mapping product ids

**Product id strings (App Store / Play):**
- `com.marcosaas.luminamatch.coins_small`
- `com.marcosaas.luminamatch.coins_medium`
- `com.marcosaas.luminamatch.coins_large`
- `com.marcosaas.luminamatch.lives_refill`
- `com.marcosaas.luminamatch.booster_pack`
- `com.marcosaas.luminamatch.remove_ads`
- `com.marcosaas.luminamatch.starter_pack`

- [ ] **Step 1: Add package `com.unity.purchasing` via Package Manager**
- [ ] **Step 2: Implement UnityIapService (init, Purchase, price locale)**
- [ ] **Step 3: Grant logic shared static `IapGrants.Apply(id, progress)` used by sandbox+unity**
- [ ] **Step 4: Hub switches implementation**
- [ ] **Step 5: Document SKUs**
- [ ] **Step 6: Commit**

```bash
git add Assets/Scripts/Monetization Packages/manifest.json docs/STORE_IAP_SKUS.md
git commit -m "$(cat <<'EOF'
feat(iap): wire Unity Purchasing with shared grant helper

EOF
)"
```

---

### Task 16: Unity Ads produção

**Files:**
- Create: `Assets/Scripts/Monetization/UnityAdsService.cs`
- Modify: `IAdsService.cs` if needed
- Modify: `MonetizationHub.cs`
- Create: `docs/STORE_ADS_SETUP.md` (Game ID placeholders)

- [ ] **Step 1: Add Advertisement package / LevelPlay as required by current Unity Ads docs for 6000.5**
- [ ] **Step 2: Implement rewarded + interstitial with RemoveAds gate**
- [ ] **Step 3: Cap interstitial: at most 1 per 180s and never during tutorial (`TutorialStep < 3`)**
- [ ] **Step 4: Editor keeps SandboxAdsService**
- [ ] **Step 5: Commit**

```bash
git add Assets/Scripts/Monetization docs/STORE_ADS_SETUP.md
git commit -m "$(cat <<'EOF'
feat(ads): Unity Ads rewarded/interstitial with frequency caps

EOF
)"
```

---

## Slice 4 — Polish, calibração, submit

### Task 17: Home/Map/Palace UI premium (fatiar UiRoot)

**Files:**
- Create screens under `Assets/Scripts/UI/Screens/`
- Shrink `UiRoot` to navigator

- [ ] **Step 1: Extract HomeScreen with palace visual + Play**
- [ ] **Step 2: Extract MapScreen**
- [ ] **Step 3: Extract ResultsScreen (win/lose/continue)**
- [ ] **Step 4: Play Mode full loop**
- [ ] **Step 5: Commit**

```bash
git add Assets/Scripts/UI
git commit -m "$(cat <<'EOF'
refactor(ui): split UiRoot into focused screen presenters

EOF
)"
```

---

### Task 18: Calibração econômica + copy pt-BR

**Files:**
- Modify: `PlayerProgress` constants if needed
- Modify: UI strings
- Update: `docs/GDD.md` to match ship reality

- [ ] **Step 1: Play 20 levels; note continue rate**
- [ ] **Step 2: Adjust coin rewards / continue only if stuck wall too hard**
- [ ] **Step 3: Sync GDD**
- [ ] **Step 4: Commit**

```bash
git add Assets/Scripts docs/GDD.md
git commit -m "$(cat <<'EOF'
chore: calibrate economy copy and sync GDD for store build

EOF
)"
```

---

### Task 19: QA + builds

**Files:** none required; use existing build scripts

- [ ] **Step 1: EditMode tests all green**
- [ ] **Step 2: Android AAB internal test**
- [ ] **Step 3: iOS archive on macOS estável (not 27 beta) → bump build number → TestFlight**
- [ ] **Step 4: Device checklist: tutorial, match4 power, palace reveal, starter offer, rewarded continue**
- [ ] **Step 5: Update `docs/STORE_STATUS.md`**
- [ ] **Step 6: Commit docs status**

```bash
git add docs/STORE_STATUS.md
git commit -m "$(cat <<'EOF'
docs: record competitive build QA and store upload status

EOF
)"
```

---

### Task 20: Submit lojas

- [ ] **Step 1: Create ASC products matching `docs/STORE_IAP_SKUS.md`**
- [ ] **Step 2: Play Console IAP + ads app id**
- [ ] **Step 3: Attach iOS build (stable toolchain) + submit review**
- [ ] **Step 4: Promote Play internal → closed/production when ready**
- [ ] **Step 5: Final status commit**

```bash
git add docs/STORE_STATUS.md docs/STORE_CHECKLIST.md
git commit -m "$(cat <<'EOF'
docs: mark competitive store submission submitted

EOF
)"
```

---

## Spec coverage checklist (self-review)

| Spec requirement | Task(s) |
|------------------|---------|
| Explicação/objetivo claro em 30–60s | 5, 6, 12 |
| Board Royal Match (gemas, moldura) | 2, 3, 4, 7 |
| Power-ups 4/5/L-T | 1, 8, 9, 10 |
| Boosters inventário UX | 5, 14 |
| Palácio Meshy visível | 12 |
| Tutorial 1–3 | 6 |
| Level overrides | 11 |
| IAP reais + starter | 13–15 |
| Ads Unity + caps | 16 |
| Oferta diária | 13, 14 |
| Monetização moderada | 6, 16, 18 |
| UI fatiada | 17 |
| Submit + ITMS-90111 mitigation | 19, 20 |

**Placeholders:** nenhum TBD intencional; SKUs de ads usam placeholders em `docs/STORE_ADS_SETUP.md` preenchidos na Task 16 com IDs reais da dashboard.

**Type consistency:** `BoardPowerType` / `PowerSpawn` / `IapProductId.StarterPack` / `OfferService` usados de forma uniforme nas tasks acima.
