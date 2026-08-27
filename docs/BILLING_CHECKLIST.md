# Cobrança real — o que falta (Marco)

O app (**0.1.41**) já cobra de verdade **quando** a loja/ads existirem.
Release **não** dá item grátis se IAP/Ads falharem.

## Feito no código

| Canal | Estado |
|-------|--------|
| Soft (moedas) | +1 vida 350 · encher 1200 · martelo/troca 200 · linha 250 |
| Continue | 750 moedas ou vídeo |
| IAP | 7 SKUs via Unity Purchasing 4.15.1 (`STORE_IAP_SKUS.md`) |
| Ads | AdMob via `AdsConfig.json` (**IDs reais**, publisher `5377628561978409`) |
| Restore | botão na loja (non-consumables); Android reporta só se houver entitlement |
| Apple IAP | 7 produtos READY_TO_SUBMIT no ASC |
| Signing | Release exige `LUMINA_STOREPASS` / `LUMINA_KEYPASS` (sem senha no git) |

## Só você (bloqueia cobrança / publicação aberta)

### 1. Google Play merchant

1. Abrir: https://play.google.com/console/u/0/developers/6604076546202815303/app/4972110585725182702/monetization-setup
2. Conta de comerciante Google Payments (banco + identidade).
3. Sem isso IAP real não cobra (produtos já criados).

### 2. Criar os 7 produtos Play

**Feito 2026-08-15.** Android usa `...coins_small1`. Apple `...coins_small`.
`remove_ads` BR: **BRL 49,99** (Play) / R$ 49,90 (Apple).

### 3. AdMob

**Feito.** IDs em `AdsConfig.json`. Vincular apps às listagens das lojas + Pagamentos no AdMob.

### 4. Tracks Play

**Feito 2026-08-21** — **0.1.41 / 49** em `internal` + `alpha`.
**Pendente:** promover `production` → `python3 tools/play_promote_production.py --dry-run` depois `--yes`.

### 5. Play Billing Library

Purchasing **4.15.1** → Billing **8.3.0**. Conferir se o aviso da Console sumiu após a publicação 49.

### 6. App Store

Upload build **50** em Mac estável + Submit for Review com os 7 IAPs (`docs/IOS_SUBMIT_RUNBOOK.md`).
