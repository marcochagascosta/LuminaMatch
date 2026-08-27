# App Store Connect — submit runbook (iOS)

App id: **6791448071** · Bundle: `com.marcosaas.luminamatch` · Team: `6LQQD54JHB`  
Target: version marketing **0.1.41** · buildNumber **50**

## Pré-requisitos

1. **macOS estável** (não 27 beta) — beta → ITMS-90111.
2. Unity **6000.5.x** + Xcode com Team `6LQQD54JHB`.
3. Env de signing Android **não** é necessário para iOS.
4. Opcional API: `ASC_ISSUER_ID`, `ASC_KEY_ID`, `ASC_PRIVATE_KEY_P8` (para automação).

## Build

```bash
# No Mac, no projeto:
# Unity → Lumina Match → Build iOS Xcode Project
# Saída: Builds/iOS/

cd Builds/iOS
# Abrir o .xcodeproj / workspace no Xcode
# Product → Archive → Distribute App → App Store Connect → Upload
# Ou: xcodebuild + Transporter com o .ipa
```

Plist pós-build (automático):

- `GADApplicationIdentifier` (AdMob)
- `ITSAppUsesNonExemptEncryption` = **false**
- `NSUserTrackingUsageDescription`
- SKAdNetwork items

## ASC (UI)

1. https://appstoreconnect.apple.com/apps/6791448071/appstore
2. Versão 1.0 (ou criar 0.1.41) — colar copy de `docs/STORE_LISTING.md`
3. Privacy Policy URL (raw GitHub)
4. Export compliance → **No**
5. Anexar build **50** quando processada
6. IAPs (7) → incluir na submissão
7. **Add for Review** → **Submit**

## ASC (API helper)

```bash
# Gera JWT e mostra endpoints — não faz submit sem confirmação humana.
python3 tools/asc_prepare_submit.py --dry-run
```

## Bloqueios conhecidos

| Erro | Ação |
|------|------|
| ITMS-90111 | Trocar para macOS estável / Xcode Cloud |
| INVALID_BINARY | Remover binary antiga e anexar build 50 |
| Missing IAP screenshot | Usar `docs/store/iap-icons/` |
