# Lumina Match

Match-3 mobile game (Unity C#) inspired by the monetization model of top-grossing casual titles like Royal Match.

Restore the **Palácio de Luz** by matching gems across 60 levels. Lives, boosters, continues, IAP sandbox and rewarded ads included.

## Requirements

- Unity **6000.5.x** (Unity 6) or compatible Unity 6 LTS
- macOS for iOS builds (Xcode)
- Android SDK via Unity Hub modules

## Open & play

1. Open `/Users/marcocosta/Games/LuminaMatch` in Unity Hub
2. Menu **Lumina Match → Setup Project Scenes** (creates `Assets/Scenes/Boot.unity` + bundle IDs)
3. Open `Assets/Scenes/Boot` and press **Play**
4. Home → Jogar → pick a level (or Continuar)

The app also auto-boots via `RuntimeInitializeOnLoadMethod` if the scene is empty.

## Tests

- Menu **Lumina Match → Run Edit Mode Tests Now**, or Window → General → Test Runner → EditMode → Run All
- Covered: match detection, gravity, level catalog, session bootstrap

## Monetization (sandbox)

`SandboxIapService` / `SandboxAdsService` grant rewards locally for testing.

Product IDs (wire to Unity IAP / store consoles before release):

| Product | Suggested price |
|---------|-----------------|
| CoinsSmall (500) | R$ 6,90 |
| CoinsMedium (1500) | R$ 14,90 |
| CoinsLarge (5000) | R$ 39,90 |
| LivesRefill | R$ 6,90 |
| BoosterPack | R$ 14,90 |
| RemoveAds | R$ 19,90 |

## Builds

- **Android APK:** Lumina Match → Build Android APK (Debug) → `Builds/Android/LuminaMatch-debug.apk`
- **iOS:** Lumina Match → Build iOS Xcode Project → `Builds/iOS/` then archive in Xcode with your Apple team

Bundle ID: `com.marcosaas.luminamatch`

## Security

Never commit keystores, `.p12`, AuthKey `.p8`, or Play service accounts. See `.gitignore`.

## Docs

- `docs/GDD.md` — game design short
- `docs/STORE_CHECKLIST.md` — App Store / Play submission
- `docs/QA_CHECKLIST.md` — manual QA gate before release
