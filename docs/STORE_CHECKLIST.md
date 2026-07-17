# Store submission checklist

## Before upload (competitive `0.1.2`)

- [x] Privacy Policy URL live (GitHub raw)
- [x] Age rating / App Privacy
- [x] Icons 1024×1024
- [x] Screenshots iPhone + iPad
- [x] Bundle ID `com.marcosaas.luminamatch`
- [x] Competitive gameplay art + powers + FTUE + palace (branch `feat/competitive-store`)
- [x] Unity IAP + Ads packages in project; production path on device builds
- [ ] Fill Unity Ads Game IDs (dashboard → `UnityAdsService` / `STORE_ADS_SETUP.md`)
- [ ] Create IAP products in ASC + Play (`STORE_IAP_SKUS.md`)
- [ ] Archive iOS on **macOS estável** (avoid ITMS-90111)
- [ ] Upload Android AAB versionCode **3** / version **0.1.2** (arquivo local pronto)
- [ ] Upload iOS build **4** / version **0.1.2**

## App Store Connect

- [x] App record + Games/Puzzle/Casual
- [x] Listing + privacy + copyright + free price
- [x] TestFlight validated on device
- [x] App Privacy published
- [x] Contact phone App Review (`+55 37 99988-0812`)
- [ ] New submission with competitive build 4 (macOS estável)
- [ ] Clear prior INVALID_BINARY / attach new binary

## Google Play Console

- [x] Internal testing AAB `0.1.1` / versionCode `2`
- [x] Listing + forms + testers
- [ ] Upload competitive AAB `0.1.2` / versionCode `3`
- [ ] Validate install on Android device
- [ ] Merchant account (for real IAP)
- [ ] Closed test → Production

## Auth (do NOT paste passwords in chat)

- Apple: Xcode / Transporter / ASC API key
- Google: Play Console + local keystore
- GitHub: `gh auth login`
