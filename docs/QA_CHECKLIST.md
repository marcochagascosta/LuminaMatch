# QA checklist — pronto para teste

## Editor

- [ ] Project opens without compile errors
- [ ] Play mode shows Home with Lumina Match branding
- [ ] Level select lists 60 levels; only unlocked are playable
- [ ] Start level spends 1 life
- [ ] Swap adjacent gems that form a match clears and cascades
- [ ] Invalid swap is rejected (no move spent)
- [ ] Complete objectives → vitória → coins + castle progress
- [ ] Run out of moves → result with continue (coins / video)
- [ ] Shop sandbox purchases increase coins/lives/boosters
- [ ] Out of lives → rewarded ad grants +1 life
- [ ] Edit Mode tests: all green

## Android debug

- [ ] APK installs on device
- [ ] Portrait UI usable with fingers
- [ ] No crash on 10 consecutive levels

## iOS (TestFlight later)

- [ ] Xcode archive succeeds with your team
- [ ] Touch + safe area OK
