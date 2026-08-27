# Play Console — criar 7 IAPs (colar)

Pré-requisito: conta de comerciante Google Payments em
[https://play.google.com/console/u/0/developers/6604076546202815303/app/4972110585725182702/monetization-setup](https://play.google.com/console/u/0/developers/6604076546202815303/app/4972110585725182702/monetization-setup)

Depois: Produtos únicos →
[https://play.google.com/console/u/0/developers/6604076546202815303/app/4972110585725182702/one-time-products](https://play.google.com/console/u/0/developers/6604076546202815303/app/4972110585725182702/one-time-products)

Package: `com.marcosaas.luminamatch`  
Tipo: compra única. Consumível = sim (exceto os 2 últimos).  
Preço BRA = mesmo da Apple.


| Product ID (exato)                       | Consumível | Preço BRA | Título pt-BR      | Título en-US | Descrição pt-BR                                          |
| ---------------------------------------- | ---------- | --------- | ----------------- | ------------ | -------------------------------------------------------- |
| `com.marcosaas.luminamatch.coins_small`  | sim        | R$ 6,90   | 500 moedas        | 500 coins    | Um saco pequeno de moedas para continuar e boosters.     |
| `com.marcosaas.luminamatch.coins_medium` | sim        | R$ 14,90  | 1500 moedas       | 1500 coins   | Pacote médio de moedas. Melhor valor do dia a dia.       |
| `com.marcosaas.luminamatch.coins_large`  | sim        | R$ 39,90  | 5000 moedas       | 5000 coins   | Grande reserva de moedas para o Palácio de Luz.          |
| `com.marcosaas.luminamatch.lives_refill` | sim        | R$ 6,90   | Recarregar vidas  | Refill lives | Enche as vidas até o máximo.                             |
| `com.marcosaas.luminamatch.booster_pack` | sim        | R$ 14,90  | Pacote de poderes | Booster pack | +3 martelo, +3 troca e +3 linha.                         |
| `com.marcosaas.luminamatch.remove_ads`   | **não**    | R$ 49,90  | Remover anúncios  | Remove ads   | Remove interstitials. Compra permanente.                 |
| `com.marcosaas.luminamatch.starter_pack` | **não**    | R$ 9,90   | Pacote estreia    | Starter pack | 2000 moedas, +5 de cada booster e vidas cheias. Uma vez. |


## Ícones (ordem da tabela)

Pasta: `docs/store/iap-icons/` e `~/Desktop/LuminaMatch-iap-icons/`


| #   | Product ID        | Arquivo                |
| --- | ----------------- | ---------------------- |
| 1   | `...coins_small`  | `iap_coins_small.png`  |
| 2   | `...coins_medium` | `iap_coins_medium.png` |
| 3   | `...coins_large`  | `iap_coins_large.png`  |
| 4   | `...lives_refill` | `iap_lives_refill.png` |
| 5   | `...booster_pack` | `iap_booster_pack.png` |
| 6   | `...remove_ads`   | `iap_remove_ads.png`   |
| 7   | `...starter_pack` | `iap_starter_pack.png` |


Play Console: ícone do produto (PNG quadrado). Se pedir 512×512, redimensionar no Preview.

Ativar cada produto e publicar na Internal Testing.
Conta de teste licenciada: Play Console → Configurações → Licenças.