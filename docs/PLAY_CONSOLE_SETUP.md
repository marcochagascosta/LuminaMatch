# Google Play Console — Lumina Match (guia de preenchimento)

Package name: **`com.marcosaas.luminamatch`**

## 1. Store listing (Crescimento → Presença na loja → Listagem principal)

| Campo | Valor |
|-------|-------|
| Nome do app | Lumina Match |
| Descrição curta (≤80) | Combine gemas e restaure o Palácio de Luz. Puzzle casual com boosters! |
| Descrição completa | (copiar de `docs/STORE_LISTING.md` — seção Full description, em PT se preferir) |
| Ícone do app | `Desktop/LuminaMatch-icon-1024.png` |
| Gráfico de recursos | opcional no Internal; use ícone ampliado ou screenshot |
| Screenshots telefone | `Desktop/LuminaMatch-screenshots/iphone67_*.png` (redimensionar se pedir tamanho Android) |
| Categoria | Jogos → Quebra-cabeça |
| E-mail de contato | marcochagascosta@gmail.com |
| Política de privacidade | https://raw.githubusercontent.com/marcochagascosta/LuminaMatch/main/docs/PRIVACY_POLICY.md |

## 2. Classificação de conteúdo (Política → Conteúdo do app)

Questionário típico para match-3 casual:

- Violência: **Não** / mínima cartoon
- Sexualidade: **Não**
- Linguagem imprópria: **Não**
- Drogas: **Não**
- Jogos de azar: **Não** (IAP de moedas não é cassino)
- Interação entre usuários: **Não**
- Compartilhamento de localização: **Não**
- Compras no app: **Sim** (moedas, vidas, boosters — sandbox por enquanto)
- Anúncios: **Sim** (sandbox/placeholder)

Resultado esperado: **Livre** / PEGI 3 / Everyone

## 3. Segurança dos dados (Política → Segurança dos dados)

Para o MVP atual (progresso local, sem login):

| Pergunta | Resposta |
|----------|----------|
| Coleta ou compartilha dados? | **Sim** (mínimo — ver abaixo) ou **Não** se o formulário permitir declarar zero coleta |
| Dados coletados | Preferir **Nenhum dado coletado** se o app só salva localmente |
| Se pedir IAP/analytics futuros | Compras no app processadas pelo Google Play; não armazenamos cartão |
| Criptografia em trânsito | Sim (HTTPS quando houver rede) |
| Exclusão de dados | Usuário desinstala o app |
| Política de privacidade | URL acima |

## 4. Público-alvo (Política → Público-alvo)

- Público: **13+** ou **Todos** (sem conteúdo infantil dirigido)
- Não é app **Somente para crianças**
- Não participa do programa Designed for Families (por enquanto)

## 5. Apps governamentais / COVID / notícias

- Todos **Não**

## 6. Recursos financeiros

- **Não** (não é app de carteira/câmbio)

## 7. Teste interno (Testar e lançar → Teste interno)

1. **Criar nova versão**
2. Upload: `/Users/marcocosta/Desktop/LuminaMatch-release.aab`
3. Nome da versão: `0.1.1 (2)`
4. Notas: `Primeira build interna — match-3 MVP, economia e loja sandbox.`
5. **Revisar versão** → **Iniciar implantação para testadores internos**
6. Aba **Testadores** → adicione seu e-mail Google
7. Copie o link **opt-in** e instale no Android

## 8. Assinatura do app (Integridade do app)

O AAB já está assinado com:
- Keystore: `~/.lumina-match-secrets/lumina-upload.keystore`
- Alias: `lumina_upload`

Na primeira upload, o Play registra a **chave de upload**. Guarde o keystore — perder = não atualiza o app.

## Checklist rápido antes do Internal

- [ ] Package `com.marcosaas.luminamatch` confere
- [ ] Política de privacidade URL salva
- [ ] Classificação de conteúdo concluída
- [ ] Segurança dos dados concluída
- [ ] AAB uploaded
- [ ] Testador adicionado
