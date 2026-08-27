# Lançar agora — faturar sem depender do Macbook beta

**Objetivo:** começar a faturar o mais cedo possível.  
**Android não precisa de Mac.** iOS usa CI macOS estável (GitHub Actions).

## Ordem (faça nesta sequência)

### 1) Google Play Production (faturamento Android)

1. Crie uma **service account** no GCP projeto `lumina-match-play47` com API `androidpublisher` + acesso na Play Console ao app Lumina Match.
2. Cole o JSON em:
   - Cursor secrets: `GOOGLE_APPLICATION_CREDENTIALS_JSON`
   - GitHub → Settings → Secrets → Actions: mesmo nome
3. Confirme merchant:  
   https://play.google.com/console/u/0/developers/6604076546202815303/app/4972110585725182702/monetization-setup
4. Rode o workflow **Promote Play Production** (digite `PROMOTE`)  
   ou localmente: `python3 tools/play_promote_production.py --yes`
5. Isso promove o AAB **0.1.41 / versionCode 49** (já em alpha/internal) para **production**.

### 2) iOS / TestFlight (Mac CI — contorna o beta)

Seu Macbook no macOS 27 beta **não** faz upload (ITMS-90111). O workflow **iOS TestFlight (macOS CI)** roda em `macos-14` estável.

Secrets GitHub Actions:

| Secret | Obrigatório |
|--------|-------------|
| `UNITY_LICENSE` ou `UNITY_EMAIL`+`UNITY_PASSWORD` | sim |
| `ASC_ISSUER_ID` | sim |
| `ASC_KEY_ID` | sim |
| `ASC_PRIVATE_KEY_P8` | sim |
| certs/profile Apple (P12 + provisioning) | só se automatic signing falhar |

Depois: Actions → **iOS TestFlight (macOS CI)** → Run.  
Build alvo: marketing **0.1.42** · iOS build **51** (fix iPad).

No ASC: quando a build 51 processar → TestFlight → (opcional) Submit for Review.

### 3) AdMob

Vincule os apps AdMob às listagens das lojas e conclua Pagamentos no AdMob.

## O que o agente já deixou pronto

- Código + fix iPad no branch `cursor/store-production-ready-d4b7` / PR #4
- `tools/play_promote_production.py`
- `.github/workflows/promote-play-production.yml`
- `.github/workflows/ios-testflight.yml`

## Bloqueio atual

Sem os secrets acima o agente **não consegue** chamar Play/ASC nem licenciar Unity no CI.  
Cole os secrets e dispare os workflows (ou avise o agente para rodar o promote Play).
