# Lumina Match — GDD (MVP)

## Fantasy

You restore the **Palácio de Luz**, a night palace whose rooms awaken as you clear gem levels.

## Core loop

1. Spend 1 life → play Match-3 level
2. Meet objectives within move limit
3. Earn coins + progress castle pieces (1 piece / 3 wins)
4. On fail → continue with coins, IAP, or rewarded ad

## Board

- 8×8 grid, 4–6 gem colors by difficulty
- Ice (needs match on cell) and Box (cleared by adjacent match) blockers from mid-game
- Boosters: Hammer (destroy one), Swap (force adjacent swap), Line blast (clear row)

## Content

- 60 procedurally seeded levels (`LevelCatalog`)
- Linear map unlock
- 20 castle pieces meta

## Economy

- Max 5 lives, regen every 20 minutes
- Soft currency: coins
- Continue cost: 900 coins / +5 moves

## Out of scope (post-MVP)

Weekly tournaments, battle pass, social, cloud save, 200+ handcrafted levels
