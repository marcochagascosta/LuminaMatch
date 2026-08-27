# Store ads setup (AdMob)

IDs de runtime: `Assets/Resources/Monetization/AdsConfig.json`  
Conta: `ca-app-pub-5377628561978409`

| Campo | Valor |
|-------|--------|
| `androidAppId` | `ca-app-pub-5377628561978409~5051039283` |
| `iosAppId` | `ca-app-pub-5377628561978409~6523119230` |
| `rewardedAndroid` | `ca-app-pub-5377628561978409/5397739823` |
| `rewardedIos` | `ca-app-pub-5377628561978409/3874417938` |
| `interstitialAndroid` | `ca-app-pub-5377628561978409/1487198798` |
| `interstitialIos` | `ca-app-pub-5377628561978409/1977369563` |

Apps **Lumina Match** Android + iOS criados na AdMob (ainda sem loja vinculada / “Requer revisão”). A conta está em verificação (até 24h). Até a aprovação, a veiculação pode ficar limitada.

## Onde ver o dinheiro

Não é no Play Console. É no **AdMob**:

1. https://admob.google.com → **Pagamentos** (e o painel inicial com ganhos estimados).
2. Relatórios por app/unidade em **Relatórios**.
3. O pagamento sai pela conta **AdSense / pagamentos Google** ligada à AdMob (banco cadastrado lá).

## Quando cai na conta

- Ganhos do mês são **estimados** no painel; fecham no começo do mês seguinte.
- Se o saldo finalizado passar de **US$ 100** (ou equivalente) e não houver bloqueio, o Google emite o pagamento **por volta do dia 21** do mês seguinte.
- Abaixo de US$ 100, o saldo **acumula** para o mês posterior.
- Primeiro pagamento: precisa PIN pelo correio, dados fiscais e forma de pagamento. Pode atrasar semanas.

## Runtime

- Editor → `SandboxAdsService` (recompensa instantânea, sem vídeo).
- Player → `AdMobAdsService`.
- Interstitial: após derrota; 1 a cada 180s; nunca no tutorial (`TutorialStep < 3`); bloqueado se comprou Remover anúncios.
- Rewarded: botão na derrota e na tela Sem vidas.
