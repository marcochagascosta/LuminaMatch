# Palace Meshy prompts

Night crystal palace exterior for Lumina Match meta progression. Import outputs as `Assets/Art/Palace/stage_0` … `stage_4` (FBX/GLB or PNG hero render). Load at runtime via `Resources.Load(PalaceStageVisual.ResourcePath(index))`.

## Base prompt

```text
Night crystal palace exterior, soft gold trim, luminous windows,
fantasy mobile game hero prop, centered, clean silhouette,
game-ready low poly, PBR materials, no characters, no text,
dark blue night sky, subtle magical glow, Royal Match premium feel
```

## Keyframes (5 stages)

| File | Stage | Prompt suffix |
|------|-------|---------------|
| `stage_0` | Ruínas | crumbling crystal ruins, broken walls, faint inner light, overgrown night garden |
| `stage_1` | Portão de Cristal | restored crystal gate, twin pillars, warm gold filigree, first luminous windows |
| `stage_2` | Torres | gate plus two tall crystal towers, amber and sapphire window glow, wider courtyard |
| `stage_3` | Cúpula | full palace body, central amethyst dome, bridges of light, nearly complete |
| `stage_4` | Coroa do Palácio | majestic completed palace, crowned dome, radiant crown spire, maximum gold and crystal sparkle |

## Export notes

- Target: low poly (&lt; 15k tris per stage) or 1024×1024 PNG render on transparent/dark gradient.
- Consistent camera: 3/4 front-right, same scale across stages.
- Pieces 0–20 interpolate visually between keyframes in UI (`PalaceStageVisual.StageIndexFromPieces`).

## Negative prompt

```text
daytime, modern architecture, people, animals, UI overlay, watermark, blurry, low resolution
```
