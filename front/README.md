# Oportuniza · Front-end

Aplicação web do **Oportuniza**, que consome a API .NET deste repositório (`src/Oportuniza.Api`).
Cobre **todas** as funcionalidades da API, com telas para os três perfis (cidadão, organização e administração).

**Stack:** React 19 · TypeScript 5.9 · Vite 8 · React Router 7 · TanStack Query 5 · Tailwind CSS 4 · Recharts 3 · Motion · Vitest + Testing Library · Playwright.

## Como rodar

Pré-requisitos: **Node 20+** e a **API rodando** (ver o README da raiz), acessível em `http://127.0.0.1:5080`.

```bash
# 1) API (na raiz do repositório)
$env:ASPNETCORE_ENVIRONMENT="Development"     # PowerShell
dotnet run --project src/Oportuniza.Api

# 2) Front (nesta pasta)
cd front
npm install
npm run dev            # http://localhost:5173
```

Para apontar para outra API, copie `.env.example` para `.env` e altere `VITE_API_URL`.

**Origens (importante):**
- Abra o front em **`http://localhost:5173`**. É a única origem liberada no CORS da API (`Cors:OrigensPermitidas`); `http://127.0.0.1:5173` é bloqueado.
- Acesse a API por **`http://127.0.0.1:5080`**. Com a API no Docker, `localhost` pode não responder. Nesse caso use `VITE_API_URL=http://127.0.0.1:5080`
  (no `.env`, ou na linha de comando; no PowerShell: `$env:VITE_API_URL="http://127.0.0.1:5080"` antes do comando).
- A API também sobe via Docker (`docker compose up -d --build` na raiz; seção 4 do README da raiz).

| Comando | O que faz |
|---|---|
| `npm run dev` | Servidor de desenvolvimento (porta 5173) |
| `npm run build` | Checagem de tipos + build de produção em `dist/` |
| `npm run preview` | Serve o build de produção |
| `npm test` | Testes unitários e de componentes (Vitest) |
| `npm run test:coverage` | Testes com relatório de cobertura (`coverage/`) |
| `npm run e2e` | Testes ponta a ponta (Playwright) **contra a API real** |

**Usuários de demonstração** (senha `Senha@123`): `ana@exemplo.local` (cidadã), `contato@recomecar.local` (organização), `admin@oportuniza.local` (admin).
A tela de login tem botões de acesso rápido com eles.

## Telas e funcionalidades

| Tela | Rota | Perfil | Endpoints usados |
|---|---|---|---|
| **Início** com hero animado, indicadores ao vivo, áreas, oportunidades em destaque, propostas mais votadas e ODS 4/8/10/11 | `/` | público | `GET /indicadores/impacto`, `GET /oportunidades`, `GET /organizacoes`, `GET /propostas` |
| **Busca de oportunidades** com texto, área, tipo, modalidade, bairro, gratuitas e encerradas; filtros na URL (dá para compartilhar o link) e paginação | `/oportunidades` | público | `GET /oportunidades` |
| **Detalhe da oportunidade**: inscrição (com confete 🎉), barra de vagas, prazo; para a dona/admin: editar, encerrar, cancelar, ver inscritos | `/oportunidades/:id` | público / cidadão / org. | `GET /oportunidades/{id}`, `POST /oportunidades/{id}/inscricoes`, `PATCH .../encerrar`, `PATCH .../cancelar` |
| **Propostas da comunidade**: ranking por votos, filtros, votar, criar proposta | `/propostas` | público / cidadão | `GET /propostas`, `POST /propostas`, `POST /propostas/{id}/votos` |
| **Detalhe da proposta**: votar, arquivar (autor/admin), **atender** (organização publica a oportunidade vinculada) | `/propostas/:id` | todos | `GET /propostas/{id}`, `POST .../atender`, `PATCH .../arquivar` |
| **Organizações** e página pública de cada uma, com as oportunidades dela | `/organizacoes`, `/organizacoes/:id` | público | `GET /organizacoes`, `GET /organizacoes/{id}`, `GET /oportunidades?organizacaoId=` |
| **Painel de impacto**: taxa de conclusão, inscrições por bairro, vagas por bairro e demanda reprimida, cada gráfico com **tabela alternativa** | `/impacto` | público | `GET /indicadores/impacto` |
| **Entrar** com mensagem genérica de erro (não revela se o e-mail existe) e aviso de bloqueio por força bruta | `/entrar` | público | `POST /auth/login` |
| **Cadastro** de cidadão ou organização, **ViaCEP preenche o endereço**, medidor de **senha forte** (mesma política da API), erros da API exibidos no campo certo | `/cadastro` | público | `POST /cidadaos`, `POST /organizacoes`, `GET /enderecos/{cep}` |
| **Minha área** (cidadão): recomendações **com pontuação e motivos**, inscrições (limite de 3 ativas) com cancelamento, minhas propostas, perfil e desativação de conta | `/minha-area` | cidadão | `GET /cidadaos/{id}/recomendacoes`, `GET/PUT/DELETE /cidadaos/{id}`, `GET /cidadaos/{id}/inscricoes`, `PATCH /inscricoes/{id}/cancelar` |
| **Painel da organização**: oportunidades com totais, pedidos da comunidade para atender, perfil | `/organizacao` | organização | `GET /oportunidades?organizacaoId=`, `GET /propostas?status=Aberta`, `GET/PUT /organizacoes/{id}` |
| **Nova / editar oportunidade** | `/organizacao/oportunidades/nova`, `.../:id/editar` | organização / admin | `POST /oportunidades`, `PUT /oportunidades/{id}` |
| **Inscritos**: confirmar, concluir (entra no indicador de impacto) e cancelar | `/organizacao/oportunidades/:id/inscritos` | organização / admin | `GET /oportunidades/{id}/inscricoes`, `PATCH /inscricoes/{id}/confirmar\|concluir\|cancelar` |
| **Administração**: visão geral com gráficos, cidadãos (lista sem e-mail, detalhe, desativar), organizações (desativar), moderação de propostas | `/admin` | admin | `GET /cidadaos`, `GET/DELETE /cidadaos/{id}`, `GET/DELETE /organizacoes/{id}`, `PATCH /propostas/{id}/arquivar` |

## Arquitetura

```
front/src
├── api/          types.ts (espelha o OpenAPI) · client.ts (fetch, ApiError, token) · services.ts (um objeto por recurso)
├── auth/         AuthProvider (sessão JWT, expiração, logout no 401) · RequireAuth (rota por perfil)
├── components/
│   ├── ui/       Button, Field, Modal, Toast, Badge, Pagination, Skeleton...
│   ├── layout/   Header (menu por perfil, modo escuro), Footer, Layout (skip link)
│   └── ...       OportunidadeCard, PropostaCard, OportunidadeForm, EnderecoFields (ViaCEP), ImpactCharts...
├── hooks/        useTheme, useDebounce, useVotos
├── lib/          labels (rótulos pt-BR dos enums), format (datas, CEP), senha (política da API)
└── pages/        uma página por rota, carregada sob demanda (lazy), separadas por perfil
```

- **Estado do servidor** fica no TanStack Query (cache, revalidação e invalidação depois de cada ação).
- **Filtros na URL**: busca, propostas e abas usam `?parametros`, então voltar, recarregar e compartilhar funcionam.
- **Code splitting**: cada página é um pacote separado; os gráficos só são baixados quando alguém abre o painel.

## Segurança no front

- O token JWT fica no `localStorage` com a data de expiração: a sessão encerra sozinha quando o token expira, e qualquer **401** da API desloga.
- As rotas são protegidas por perfil (`RequireAuth`), mas **quem decide é a API**: o front só esconde o que o perfil não pode usar.
- Nenhum HTML vindo da API é injetado (sem `dangerouslySetInnerHTML`); o React escapa todo texto.
- O site da organização só vira link se for `http(s)` (bloqueia `javascript:`), com `rel="noopener noreferrer nofollow"`.
- Mensagem de login genérica, sem revelar se o e-mail existe; aviso do bloqueio após 5 tentativas.
- Senha validada com a mesma regra da API (8 a 64 caracteres, letras e números, sem espaço nas pontas).

## Acessibilidade e experiência

- HTML semântico, rótulos ligados aos campos, erros anunciados com `role="alert"` e `aria-describedby`.
- Link "Pular para o conteúdo", foco visível, diálogos com foco preso e fechamento com Esc.
- Status sempre com **texto e ícone**, nunca só cor; todo gráfico tem uma **visão em tabela**.
- Paleta dos gráficos **validada para daltonismo** (protanopia, deuteranopia e tritanopia) nos modos claro e escuro.
- **Modo escuro** (segue o sistema e lembra a escolha) e respeito a `prefers-reduced-motion`.
- Layout responsivo do celular ao desktop.

## Testes

```bash
npm test                 # 91 testes (Vitest + Testing Library, API simulada)
npm run test:coverage    # cobertura: ~87% das linhas
VITE_API_URL=http://127.0.0.1:5080 npm run e2e   # 19 testes, Playwright contra a API real (suba a API antes; passam contra a API no Docker)
```

| Arquivo | O que cobre |
|---|---|
| `src/lib/lib.test.ts` | Política de senha, datas, CEP, rótulos |
| `src/api/client.test.ts` | Token Bearer, 204, erros de regra de negócio, erros de validação por campo, 401 desloga, falha de rede |
| `src/components/components.test.tsx` | Cards, votação, barra de vagas, formulário de oportunidade, gráficos com tabela |
| `src/pages/pages.test.tsx` | Login (401, 429), cadastro com ViaCEP e erros da API, busca com filtros, inscrição, regras violadas, gestão pela dona, votos, impacto, inscritos |
| `src/pages/paineis.test.tsx` | App inteiro com rotas reais: home, tema, logout, 404, organizações (bloqueio de `javascript:`), minha área, painel da organização, criar/editar, atender proposta, administração |
| `e2e/publico.spec.ts` | Visitante no desktop **e no celular**: home ao vivo, busca, impacto, modo escuro, rota protegida, login inválido |
| `e2e/fluxo-completo.spec.ts` | Fluxo real de ponta a ponta: org publica → cidadão é recomendado e se inscreve → propõe → vizinho vota (e não vota duas vezes) → org confirma, conclui e atende → cidadão vê tudo → admin vê a lista sem e-mail |

O E2E sobe o front sozinho em `http://localhost:5173` e fala com a API em `VITE_API_URL`. No PowerShell: `$env:VITE_API_URL="http://127.0.0.1:5080"; npm run e2e`.

Capturas da revisão visual (28 imagens, claro/escuro, desktop/celular): [`../evidencias/front/`](../evidencias/front/).

Os testes E2E criam usuários novos a cada execução (e-mail com data e hora), então podem rodar várias vezes sobre o mesmo banco.
