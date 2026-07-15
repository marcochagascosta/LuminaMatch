# Store submission checklist

## Before upload

- [ ] Replace sandbox IAP/Ads with production Unity IAP + AdMob/Unity Ads IDs
- [ ] Privacy Policy URL live
- [ ] Age rating questionnaire (Apple) / content rating (Play)
- [ ] Icons 1024×1024 + adaptive Android icon
- [ ] Screenshots: 6.7" iPhone, 12.9" iPad, phone + 7" tablet Android
- [ ] Bundle ID `com.marcosaas.luminamatch` matches consoles
- [ ] Version `0.1.0` / versionCode `1`

## App Store Connect

- [ ] Create app record, category Games / Puzzle
- [ ] Encryption export compliance (ITSAppUsesNonExemptEncryption = false if only HTTPS)
- [ ] TestFlight internal build approved by you
- [ ] Submit for review

## Google Play Console

- [ ] Create app, complete Data safety form
- [ ] Upload AAB (preferred) or APK to Internal testing
- [ ] Target API level per current Play policy
- [ ] Promote to production after QA OK

## Auth (do NOT paste passwords in chat)

- Apple: Xcode signed in with your Apple ID / App Store Connect API key on your Mac
- Google: Play Console browser login + local upload keystore
- GitHub: `gh auth login` on your machine
