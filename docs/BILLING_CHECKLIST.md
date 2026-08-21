# Cobrança real — o que falta (Marco)

O app (0.1.38) já cobra de verdade **quando** a loja/ads existirem.
Release **não** dá item grátis se IAP/Ads falharem.

## Feito no código

| Canal | Estado |
|-------|--------|
| Soft (moedas) | +1 vida 350 · encher 1200 · martelo/troca 200 · linha 250 |
| Continue | 750 moedas ou vídeo |
| IAP | 7 SKUs via Unity Purchasing (`STORE_IAP_SKUS.md`) |
| Ads | AdMob via `AdsConfig.json` (sample IDs = anúncio de teste, R$ 0) |
| Restore | botão na loja (non-consumables) |
| Apple IAP | 7 produtos READY_TO_SUBMIT no ASC |

## Só você (bloqueia cobrança real)

### 1. Google Play merchant (primeiro)

1. Abrir: https://play.google.com/console/u/0/developers/6604076546202815303/app/4972110585725182702/monetization-setup
2. Configurar conta de comerciante Google Payments (banco + identidade).
3. Sem isso a página de produtos fica bloqueada.

### 2. Criar os 7 produtos Play

**Feito 2026-08-15** — 7 produtos únicos na Play Console.
Pendência resolvida no app: Android usa `...coins_small1` (o ID que existe no Play). Apple continua `...coins_small`.
`remove_ads` ativo no Brasil em **BRL 49,99** (tier do Play; Apple é R$ 49,90).

### 3. AdMob (ganhos de anúncio)

**Feito 2026-08-18.** Apps Android+iOS e unidades rewarded/interstitial criadas. IDs em `AdsConfig.json`.
Conta AdMob ainda em verificação (até 24h) e apps “Requer revisão” até vincular loja. Pagamentos: https://admob.google.com → Pagamentos.

### 4. Upload Internal Testing

**Feito 2026-08-21** — **0.1.41 / versionCode 49** publicado via API do Play nas faixas `internal` e `alpha` (as duas ativas). Fluxo documentado em `STORE_STATUS.md`.

### 5. Play Billing Library (aviso da Play Console)

- Exigência: Billing Library **8.0.0+** até 30/08/2026.
- Correção no código: `Packages/manifest.json` → `com.unity.purchasing` **4.15.1** (Billing **8.3.0**). API 4.x (`IDetailedStoreListener`) permanece; não precisa migrar para IAP 5.
- O artefato 49 declara `com.google.android.play.billingclient.version = 8.3.0`. A Play reavalia por conta própria — conferir se o aviso saiu.
