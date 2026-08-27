# Lumina Match — Store Status

**Branch:** `cursor/store-production-ready-d4b7` (base `sync/0.1.41-store-ready`)  
**Updated:** 2026-08-27  
**Marketing version:** `0.1.41` · Android versionCode `49` · iOS buildNumber `50`

## Objetivo desta sessão

Deixar o código **pronto para produção** e publicar:

1. **Google Play Production** — promover o AAB já vivo em internal/alpha (mesmo versionCode 49).
2. **App Store** — checklist + plist (encryption) prontos; upload exige Mac estável (não beta).

## Builds

| Artefato | Onde | Status |
|----------|------|--------|
| Android AAB 0.1.41 / 49 | Publicado Play `internal` + `alpha` | Pronto para promover a `production` |
| iOS IPA 0.1.41 / 50 | Gerar no Mac estável | Bloqueado se macOS 27 beta (ITMS-90111) |
| EditMode | 26+ testes no histórico | Rodar no Unity 6.5 antes do archive |

## App Store Connect

- App id: `6791448071` · Team: `6LQQD54JHB`
- Version 1.0 (ou 0.1.41 conforme ASC) — anexar build **50**
- 7 IAPs READY_TO_SUBMIT (submeter com o app)
- Privacy URL: https://raw.githubusercontent.com/marcochagascosta/LuminaMatch/main/docs/PRIVACY_POLICY.md
- Encryption: **No** (`ITSAppUsesNonExemptEncryption=false` no post-process iOS)
- Contact Review: `+55 37 99988-0812`

## Google Play

- App ID: `4972110585725182702` · Developer: `6604076546202815303`
- Faixas ativas: `internal` + `alpha` com **0.1.41 / 49** (`status: completed`)
- `production` vazia → promover com `tools/play_promote_production.py`
- Billing Library no artefato: **8.3.0**
- Package: `com.marcosaas.luminamatch`
- Projeto API: `lumina-match-play47`

### Links

- Internal: https://play.google.com/console/u/0/developers/6604076546202815303/app/4972110585725182702/tracks/internal-testing
- Production: https://play.google.com/console/u/0/developers/6604076546202815303/app/4972110585725182702/tracks/production
- IAP: https://play.google.com/console/u/0/developers/6604076546202815303/app/4972110585725182702/one-time-products
- Merchant: https://play.google.com/console/u/0/developers/6604076546202815303/app/4972110585725182702/monetization-setup

## Monetização

| Canal | Estado |
|-------|--------|
| Soft currency | Ativo no app |
| Unity IAP | Produção em player builds; sandbox só no Editor |
| Play / ASC IAPs | 7 produtos criados |
| AdMob | IDs reais em `AdsConfig.json` (publisher `5377628561978409`) |
| Merchant Play | Confirmar na Console (bloqueia cobrança real) |
| Debug APK | **Não enviar às lojas** (libera grants locais) |

## Correções 2026-08-27 (este branch)

- Senha do keystore removida do código — exige `LUMINA_STOREPASS` / `LUMINA_KEYPASS`
- `ProjectSetup` não reseta mais versão para 0.1.1
- minSdk Android alinhado a API 26
- Restore IAP Android não retorna sucesso falso
- Defaults AdMob sem sample IDs
- Ícone 1024 quadrado + feature graphic no repo
- Listing PT-BR + scripts de promote/submit

## Ainda pendente (humano / credenciais)

1. Promover Play → production (ADC / service account)
2. Confirmar merchant Google Payments
3. Upload iOS em Mac **estável** + Submit for Review
4. Colar screenshots recentes se a Console pedir refresh
5. Vincular apps AdMob às listagens das lojas

## Privacy

`Assets/Plugins/iOS/PrivacyInfo.xcprivacy` presente. Política atualizada para AdMob.
