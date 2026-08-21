# AGENTS.md

## Propósito e estado deste documento

Este arquivo é o guia operacional durável para agentes que trabalham no Agend.AI. Ele consolida a arquitetura, a stack, a configuração, a estrutura do repositório, o estado implementado, as pendências e as regras que não podem ser inferidas apenas pelo código.

O inventário técnico foi revisado em **13 de agosto de 2026** e o estado da integração da agenda no frontend foi atualizado em **20 de agosto de 2026**. Ao alterar arquitetura, dependências, variáveis de ambiente, estrutura de diretórios ou estado funcional, atualize também as seções correspondentes deste arquivo.

O Agend.AI é um CRM para clínicas com agenda, cadastros e automação de agendamentos. A solução combina uma SPA React, uma API ASP.NET Core, PostgreSQL com NHibernate e workflows n8n integrados, no desenho de destino, ao Telegram e ao Google Calendar.

## Fontes de verdade

A documentação de produto está atualmente fora deste repositório:

`C:\Users\markn\mdsl1\Projetos Pessoais\Agend.AI`

Se futuramente existir `docs/` neste repositório, prefira a cópia versionada em `docs/`. Se nenhuma das duas localizações estiver disponível, informe a ausência antes de tomar uma decisão de produto.

Leia somente os documentos pertinentes à tarefa:

- `Agend.AI.md`: visão geral, ciclo de vida e estado macro das fases.
- `Planejamento/Objetivos e Definição de Sucesso.md`: objetivos e marcos de sucesso.
- `Planejamento/Requisitos.md`: requisitos funcionais e não funcionais, identificados por RF/RNF.
- `Planejamento/Regras de Negócio.md`: invariantes de integração, RBAC, agendamento e retenção, identificadas por RN.
- `Planejamento/Stack.md`: stack desejada; confirme no código se cada dependência já foi adotada.
- `Planejamento/Etapas do Projeto e Cronograma.md`: sequência planejada da PoC e do MVP; estimativas não representam estado implementado.
- `Diário de Bordo.md`: registro histórico do trabalho concluído.
- `Design - Arquitetura - Infraestrutura/DESIGN.md.md`: comportamento visual e especificações das telas.
- `Design - Arquitetura - Infraestrutura/Estrutura de Tabelas.md`: modelo conceitual; valide sempre contra `database/init.sql`, entidades e mappings.

Use esta precedência:

1. A solicitação atual do usuário define o escopo.
2. Regras de negócio e requisitos definem o comportamento desejado.
3. Decisões explícitas mais recentes do usuário prevalecem sobre planos antigos.
4. Código, schema e workflows representam o estado realmente implementado.
5. Cronograma e diário de bordo fornecem contexto, mas não substituem verificação no código.

Quando documentação e implementação divergirem, descreva a divergência e o impacto. Não altere silenciosamente um lado para fazê-lo coincidir com o outro. Para decisões significativas, peça direção ou registre claramente a hipótese adotada.

## Visão geral da arquitetura

### Topologia executável atual

O `docker-compose.yml` provisiona quatro serviços locais. A presença do serviço na topologia não significa que o fluxo funcional correspondente já esteja implementado.

```text
Host / navegador
    |
    +-- http://localhost:5173 --> app (React/Vite)
    +-- http://localhost:5000 --> api (ASP.NET Core)
    +-- http://localhost:5678 --> n8n
    +-- localhost:5432 --------> db (PostgreSQL, somente para desenvolvimento)

app -------- app_net -------- api -------- db_net interna -------- db
                               |
n8n -------- app_net ----------+
```

- `app`, `api` e `n8n` compartilham a rede bridge `app_net`.
- Apenas `api` e `db` participam da `db_net`, que é marcada como `internal: true`.
- Na comunicação entre contêineres, `app` e `n8n` não participam da `db_net` nem resolvem o host `db`; somente a API compartilha essa rede com o PostgreSQL.
- A porta `5432` permanece publicada no host para conveniência de desenvolvimento. Portanto, a restrição “somente a API acessa o banco” é uma invariante da aplicação, não uma garantia absoluta do host local.
- `postgres_data` persiste os dados do PostgreSQL.
- `n8n_data` persiste a configuração interna do n8n.
- `app` e `api` usam bind mounts e processos de desenvolvimento com hot reload.
- `n8n/workflows` é montado em `/data/workflows`, mas não há importação automática configurada.
- Não existem health checks. `depends_on` ordena a criação da API depois do banco, mas não aguarda o PostgreSQL ficar pronto.

### Arquitetura funcional de destino

```text
Navegador -> React -> API .NET -> PostgreSQL
                         |
                         +-> webhook n8n -> Google Calendar

Telegram -> n8n -> API .NET -> PostgreSQL
                 |
                 +-> Google Calendar
```

- React fala somente com a API.
- A API é a fronteira de autenticação, autorização, escopo de clínica, validação e persistência.
- n8n fala com a API para ler ou persistir dados; não acessa o banco diretamente.
- Integrações com Google Calendar passam pelo n8n. A API não incorpora SDK nem regras específicas do Google.
- O frontend não conhece URLs privadas de webhooks do n8n.
- PostgreSQL mantém o estado relacional e a auditoria; Google Calendar é uma integração externa, não a fonte única de verdade do CRM.

### Camadas do backend

```text
AgendAi.Domain
    ^
    +-- AgendAi.Application
    +-- AgendAi.Infrastructure
             ^
             |
AgendAi.API -+-- também referencia AgendAi.Application
```

- `AgendAi.Domain`: contém as entidades atuais e deve concentrar as regras centrais; não depende das outras camadas.
- `AgendAi.Application`: deverá conter casos de uso, contratos internos, validação e mapeamento de aplicação; hoje contém apenas o projeto e as dependências e depende de Domain.
- `AgendAi.Infrastructure`: contém a configuração/mappings NHibernate e deverá receber repositórios e gateways externos; depende de Domain.
- `AgendAi.API`: contém a composição mínima atual e deverá concentrar transporte HTTP, autenticação e autorização; referencia Application e Infrastructure.

Preserve essa direção de dependências. Regras de negócio não pertencem a controllers, componentes React ou workflows quando forem regras centrais do CRM.

### Arquitetura de dados

`database/init.sql` é o bootstrap canônico para criar um banco PostgreSQL vazio. O NHibernate não cria nem atualiza o schema.

Tabelas atuais:

- `clinicas`
- `horario_funcionamento`
- `especialidades`
- `usuarios`
- `procedimentos`
- `profissionais`
- `clientes`
- `agendamentos`

Características implementadas no schema:

- identificadores internos `BIGSERIAL` e UUIDs públicos;
- instantes persistidos em `TIMESTAMPTZ`;
- soft delete por `deleted_at` em `clinicas`, `especialidades`, `usuarios`, `procedimentos`, `clientes` e `agendamentos`;
- chaves estrangeiras compostas para impedir relacionamentos entre clínicas distintas;
- `CHECK` para tipos, cargos, durações, valores, status e intervalos válidos;
- telefone ativo de cliente deduplicado por clínica após normalização para dígitos;
- IDs externos de profissional e agendamento no Google Calendar únicos quando presentes;
- exclusão GiST que impede sobreposição de agendamentos ativos do mesmo profissional;
- índices para a grade de agenda, busca de clientes e login;
- extensões PostgreSQL `uuid-ossp` e `btree_gist`.

Limitação atual: `profissionais` e `horario_funcionamento` não possuem `deleted_at`, e algumas FKs ainda usam `ON DELETE CASCADE`. Assim, a preservação de todos os históricos é uma regra obrigatória para a aplicação, mas ainda não está totalmente garantida pelo schema. Não execute exclusão física de dados de negócio e trate a ampliação do soft delete como pendência antes do MVP.

Fluxo de alteração estrutural confirmado pelo usuário:

1. Edite `database/init.sql` como definição integral do banco novo.
2. Sincronize entidades e mappings NHibernate na mesma mudança.
3. No ambiente local, o usuário recria o volume Docker para aplicar o bootstrap desde zero.

Não crie migrations incrementais sem nova decisão explícita. Um agente nunca deve remover volumes por conta própria: a exclusão é destrutiva e exige autorização atual e explícita do usuário.

## Stack tecnológica detalhada

### Frontend (`app/`)

| Tecnologia | Versão declarada | Estado/uso |
|---|---:|---|
| Node.js | imagem `node:20-alpine` | runtime de desenvolvimento; patch não fixado |
| React | `^19.2.7` | SPA e componentes funcionais |
| React DOM | `^19.2.7` | montagem da SPA |
| TypeScript | `~6.0.2` | tipagem e build |
| Vite | `^8.1.1` | dev server e bundling |
| `@vitejs/plugin-react` | `^6.0.3` | integração React/Vite |
| Tailwind CSS | `^4.3.3` | tokens e estilos utilitários |
| `@tailwindcss/vite` | `^4.3.3` | integração Tailwind/Vite |
| FullCalendar core/react/daygrid/timegrid | `^6.1.21` | agenda semanal, semana útil e mês |
| FullCalendar Luxon 3 + Luxon | `^6.1.21` / `^3.7.2` | suporte ao fuso nomeado `America/Sao_Paulo` no FullCalendar v6 |
| Lucide React | `^1.28.0` | ícones da interface |
| Sonner | `^2.0.7` | toasts |
| ESLint | `^10.6.0` | lint |
| typescript-eslint | `^8.62.0` | regras TypeScript do ESLint |

O `package-lock.json` é a fonte das versões resolvidas. Na revisão de 13/08/2026, as principais resoluções eram React/React DOM 19.2.8, TypeScript 6.0.3, Vite 8.1.5 e ESLint 10.8.0. Não edite o lockfile manualmente.

Fontes Inter e Hanken Grotesk são carregadas pelo Google Fonts em `app/src/index.css`. Não há React Router, React Query, Axios nem Zustand instalados atualmente, embora apareçam na stack planejada.

### Backend (`api/`)

Todos os projetos usam .NET 8 (`net8.0`), nullable reference types e implicit usings. O container usa `mcr.microsoft.com/dotnet/sdk:8.0`, com patch flutuante.

| Projeto | Dependências externas atuais |
|---|---|
| `AgendAi.Domain` | nenhuma |
| `AgendAi.Application` | AutoMapper 16.2.0; FluentValidation 12.1.1 |
| `AgendAi.Infrastructure` | FluentNHibernate 3.4.1; NHibernate 5.7.0; Npgsql 10.0.3 |
| `AgendAi.API` | AutoMapper 16.2.0; FluentValidation DI 12.1.1; Microsoft.AspNetCore.OpenApi 8.0.29; Swashbuckle.AspNetCore 6.6.2 |

A API constrói um `ISessionFactory` singleton e abre uma `ISession` por escopo HTTP. Controllers estão registrados, mas não há controllers concretos. Os pacotes OpenAPI/Swagger estão referenciados, porém o middleware Swagger não está configurado.

### Banco de dados

| Tecnologia | Versão/configuração | Estado/uso |
|---|---|---|
| PostgreSQL | imagem `postgres:16-alpine` | banco local de desenvolvimento |
| `uuid-ossp` | extensão do PostgreSQL | geração de UUIDs |
| `btree_gist` | extensão do PostgreSQL | constraint de não sobreposição |
| Supabase PostgreSQL | versão não definida | destino planejado de produção; não integrado |

A tag do PostgreSQL fixa a major 16, mas não a versão minor. Preserve compatibilidade com PostgreSQL/Supabase e mantenha a conexão configurável por ambiente.

### Automação (`n8n/`)

| Tecnologia | Versão/configuração | Estado/uso |
|---|---|---|
| n8n | `docker.n8n.io/n8nio/n8n:latest` | serviço local provisionado; versão não fixada |
| Armazenamento interno | volume `n8n_data` | configuração local do n8n |
| Workflows exportáveis | bind mount `n8n/workflows:/data/workflows` | diretório vazio; sem importação automática |

Telegram, Google Calendar, Groq, Redis e Google Sheets aparecem na visão planejada, mas não estão configurados nem implementados no runtime atual.

### Contêineres e desenvolvimento

- Docker Compose Specification, sem campo legado `version`.
- Dockerfile do frontend orientado a desenvolvimento, com Vite em `0.0.0.0:5173`.
- Dockerfile da API orientado a desenvolvimento, com `dotnet watch` em `0.0.0.0:5000`.
- Redes `app_net` e `db_net`; a segunda é interna.
- Volumes nomeados `postgres_data` e `n8n_data`.
- Não há imagens de produção, reverse proxy, TLS, health checks, CI/CD ou configuração de deploy no repositório.

## Variáveis de ambiente e configuração

Nunca registre valores de `.env`, connection strings, tokens, chaves, URLs privadas de webhook ou credenciais em documentação, logs, commits ou respostas. Ao auditar configuração, mostre apenas nomes e finalidade.

O `.env` da raiz existe localmente, é ignorado pelo Git e atualmente não possui um `.env.example` versionado.

### Variáveis fornecidas pelo `.env`

| Nome | Consumidor | Finalidade | Estado |
|---|---|---|---|
| `POSTGRES_DB` | `db` e composição da conexão da `api` | nome do banco | obrigatória; sem default no Compose |
| `POSTGRES_USER` | `db` e composição da conexão da `api` | usuário do PostgreSQL | obrigatória; sem default |
| `POSTGRES_PASSWORD` | `db` e composição da conexão da `api` | senha do PostgreSQL | segredo obrigatório; sem default |
| `N8N_API_KEY` | Compose, convertido em `N8n__ApiKey` na `api` | segredo de comunicação servidor-servidor | obrigatório e enviado pela API ao n8n no header `X-AgendAi-Api-Key` |

### Variáveis injetadas nos serviços

| Nome | Serviço/origem | Finalidade | Valor/configuração atual |
|---|---|---|---|
| `ASPNETCORE_ENVIRONMENT` | `api`; Compose e `launchSettings.json` | seleciona o ambiente ASP.NET Core | `Development` |
| `ConnectionStrings__DefaultConnection` | `api`; Compose | equivale a `ConnectionStrings:DefaultConnection`; conexão NHibernate | montada com `db:5432` e `POSTGRES_*`; obrigatória |
| `N8n__ApiKey` | `api`; Compose | equivale a `N8n:ApiKey` | validada na inicialização e usada pelo cliente HTTP do gateway n8n |
| `DOTNET_USE_POLLING_FILE_WATCHER` | `api/Dockerfile` | polling do `dotnet watch` no bind mount | `1` |
| `CHOKIDAR_USEPOLLING` | `app`; Compose | polling do watcher do Vite no bind mount | `true` |
| `API_PROXY_TARGET` | `app`; Compose/Vite | destino interno do proxy `/api` no desenvolvimento conteinerizado | `http://api:5000`; no host o Vite usa `http://localhost:5000` por padrão |
| `N8N_HOST` | `n8n`; Compose | host anunciado/configurado | `localhost` |
| `N8N_PORT` | `n8n`; Compose | porta do serviço | `5678` |
| `N8N_PROTOCOL` | `n8n`; Compose | protocolo local | `http` |
| `NODE_ENV` | `n8n`; Compose | modo do runtime Node | `production` |
| `WEBHOOK_URL` | `n8n`; Compose | URL-base usada para gerar webhooks | endereço local do n8n; não copiar para produção |

Observações de configuração:

- `Program.cs` encerra a inicialização com `InvalidOperationException` se `ConnectionStrings:DefaultConnection` ou `N8n:ApiKey` estiver ausente.
- `appsettings.json` contém apenas logging e `AllowedHosts`.
- O `appsettings.Development.json` local contém apenas níveis de logging e é ignorado pelo Git.
- O código entregue ao navegador não usa variáveis `VITE_*` nem `import.meta.env`; somente `vite.config.ts` lê `process.env.API_PROXY_TARGET` no servidor de desenvolvimento.
- Não há variáveis de Telegram, Google Calendar, Groq, Redis, Google Sheets ou Supabase configuradas no repositório.
- Quando essas integrações forem implementadas, use variáveis de ambiente/secret stores e adicione somente placeholders seguros ao `.env.example`.

## Estrutura exata do projeto

Árvore lógica revisada em 13/08/2026. Foram omitidos somente artefatos gerados ou internos: `.git/`, `node_modules/`, `dist/`, `bin/`, `obj/`, `.tmp-nhibernate-audit/` e o conteúdo do `.env`.

Diretórios marcados como vazios existem no workspace, mas, por estarem vazios, não são rastreados pelo Git e não aparecerão em um clone até receberem arquivos.

```text
agend.ai/
|-- .env                                  # local e ignorado
|-- .gitignore
|-- AGENTS.md
|-- docker-compose.yml
|-- figma.css                             # referência de estilos do design
|-- api/
|   |-- .dockerignore
|   |-- Dockerfile
|   |-- AgendAi.sln
|   |-- AgendAi.API/
|   |   |-- AgendAi.API.csproj
|   |   |-- AgendAi.API.http             # ainda referencia /weatherforecast
|   |   |-- appsettings.json
|   |   |-- appsettings.Development.json  # local e ignorado
|   |   |-- Program.cs
|   |   |-- Contracts/                    # vazio
|   |   |-- Controllers/                  # vazio
|   |   |-- Infrastructure/               # vazio
|   |   `-- Properties/
|   |       `-- launchSettings.json
|   |-- AgendAi.Application/
|   |   |-- AgendAi.Application.csproj
|   |   |-- Agenda/                       # vazio
|   |   `-- Common/                       # vazio
|   |-- AgendAi.Domain/
|   |   |-- AgendAi.Domain.csproj
|   |   |-- Agendamentos/
|   |   |   `-- Agendamento.cs
|   |   |-- Clientes/
|   |   |   `-- Cliente.cs
|   |   |-- Organizacoes/
|   |   |   |-- Clinica.cs
|   |   |   `-- HorarioFuncionamento.cs
|   |   |-- Profissionais/
|   |   |   |-- Especialidade.cs
|   |   |   |-- Procedimento.cs
|   |   |   `-- Profissional.cs
|   |   `-- Usuarios/
|   |       `-- Usuario.cs
|   `-- AgendAi.Infrastructure/
|       |-- AgendAi.Infrastructure.csproj
|       |-- NHibernateHelper.cs
|       |-- Agenda/                       # vazio
|       |-- Integracoes/
|       |   `-- N8n/                      # vazio
|       `-- Mappings/
|           |-- AgendamentoMap.cs
|           |-- ClienteMap.cs
|           |-- ClinicaMap.cs
|           |-- EspecialidadeMap.cs
|           |-- HorarioFuncionamentoMap.cs
|           |-- ProcedimentoMap.cs
|           |-- ProfissionalMap.cs
|           `-- UsuarioMap.cs
|-- app/
|   |-- .dockerignore
|   |-- .gitignore
|   |-- Dockerfile
|   |-- README.md                         # README padrão do template Vite
|   |-- eslint.config.js
|   |-- index.html
|   |-- package.json
|   |-- package-lock.json
|   |-- tsconfig.app.json
|   |-- tsconfig.json
|   |-- tsconfig.node.json
|   |-- vite.config.ts
|   |-- public/
|   |   |-- favicon.svg
|   |   `-- icons.svg
|   `-- src/
|       |-- App.tsx
|       |-- index.css
|       |-- main.tsx
|       |-- components/
|       |   |-- AppSidebar.tsx
|       |   |-- Header.tsx
|       |   `-- WeeklyAgenda.tsx
|       `-- features/
|           `-- agenda/
|               `-- agendaApi.ts
|-- database/
|   `-- init.sql
`-- n8n/
    |-- scripts/                          # vazio
    `-- workflows/                        # vazio
```

## Estado implementado

### Infraestrutura local

- Compose com `app`, `api`, `db` e `n8n`.
- Hot reload do frontend e backend.
- Entre contêineres, somente API e PostgreSQL compartilham a `db_net`; a porta 5432 continua publicada no host para desenvolvimento.
- Volumes persistentes para PostgreSQL e n8n.
- Configuração de conexão por ambiente, com credenciais locais no `.env` ignorado e sem valores versionados.

### Frontend da PoC

- SPA React responsiva, sem roteamento.
- Cabeçalho e sidebar recolhível.
- Tela de agenda baseada em FullCalendar.
- Visões de semana, semana útil e mês.
- Lista de profissionais ainda estática, com consulta da agenda pelo UUID selecionado.
- Navegação por período, data selecionada e botão “Hoje”.
- Localização `pt-BR` e fuso `America/Sao_Paulo`.
- Tokens visuais teal e componentes acessíveis com foco visível e rótulos.
- Eventos carregados por período via `GET /api/agenda/{profissionalUuid}`, com adaptação ao contrato do FullCalendar.
- Requisições canceláveis e estados visuais de carregamento, erro, tentativa novamente e período vazio.
- Proxy `/api` do Vite para evitar CORS no desenvolvimento local; o destino conteinerizado é configurado por `API_PROXY_TARGET`.
- Botão “Novo agendamento” visível, porém intencionalmente desabilitado.

### Backend e persistência

- Solução .NET dividida em Domain, Application, Infrastructure e API.
- Endpoint `GET /api/agenda/{profissionalUuid}` para consultar um período da agenda por profissional.
- Consulta de agenda com contratos HTTP, caso de uso, leitura NHibernate do profissional e gateway HTTP para o n8n com timeout e erros tipados.
- Oito entidades de domínio e oito mappings FluentNHibernate.
- `ISessionFactory` NHibernate configurada para PostgreSQL e sessão por escopo HTTP.
- Schema de criação integral com oito tabelas, seeds básicos, constraints e índices.
- Campos de integração `profissionais.id_google_calendar`, `profissionais.prefixo` e `agendamentos.id_event_google_calendar` sincronizados entre schema, domínio e mappings.
- Status inicial de agendamento `pendente_integracao`, com estados de sucesso, falha, conclusão, cancelamento e falta.
- Integridade multi-clínica no banco por FKs compostas.
- Deduplicação concorrente de telefone ativo e bloqueio de sobreposição da agenda no banco.
- Soft delete por `deleted_at` nas seis tabelas que já possuem o campo; a cobertura de `profissionais`/`horario_funcionamento` e a remoção de caminhos de exclusão física permanecem pendentes.

## Pendências conhecidas

Não trate os itens abaixo como implementados apenas porque constam na documentação, em um pacote instalado ou em um diretório reservado.

### PoC funcional

- Criar os demais controllers e endpoints; atualmente existe somente a consulta de agenda por profissional/período.
- Criar os demais DTOs e contratos HTTP; a consulta de agenda já possui contratos próprios.
- Implementar os demais casos de uso, validadores e perfis AutoMapper na Application.
- Implementar repositórios, transações e filtros de soft delete no NHibernate.
- Completar a política de retenção no schema para `profissionais` e `horario_funcionamento` e revisar FKs `ON DELETE CASCADE` antes de fluxos de exclusão.
- Exportar workflows n8n importáveis para disponibilidade, criação e consulta de agenda.
- Integrar Google Calendar e definir tratamento de falha parcial/idempotência.
- Implementar o fluxo Telegram -> n8n -> API.
- Substituir a lista estática de profissionais do frontend por dados da API.
- Habilitar criação real de agendamento na interface.
- Verificar o fluxo ponta a ponta React -> API -> n8n -> Google Calendar e n8n -> API -> PostgreSQL.

### MVP

- Cadastro de clínica, login, sessão e autenticação.
- RBAC efetivo no backend e políticas de escopo por clínica.
- Restrição para profissional acessar somente a própria agenda.
- CRUDs de profissionais, usuários, clientes, especialidades e procedimentos.
- Criação de profissional condicionada ao sucesso da criação do subcalendário externo.
- Telas funcionais de Dashboard, Pacientes, Usuários e Configurações; hoje são somente itens visuais.
- Chatbot dinâmico, estado conversacional, Groq/LLM e deep links.
- Supabase como PostgreSQL de produção.
- Rate limiting, CORS, Problem Details, health checks, logs estruturados e observabilidade.
- Imagens/Compose de produção, deploy e CI/CD.

### Qualidade e manutenção

- Não existem projetos de teste .NET nem testes frontend.
- Não existe `.env.example` versionado.
- `app/README.md` ainda é o README padrão do Vite.
- `AgendAi.API.http` ainda referencia o endpoint inexistente `/weatherforecast`.
- `launchSettings.json` abre `swagger`, mas a API não registra o middleware Swagger.
- Não há health checks nem espera de prontidão do PostgreSQL.
- Imagens base usam patches flutuantes e n8n usa `latest`; avaliar fixação antes de produção.
- Remover de seeds qualquer URL real/privada de webhook e usar configuração ou placeholder local seguro.
- Definir se o relacionamento profissional-especialidade continuará 1:N ou será migrado para N:N.

## Invariantes arquiteturais e de negócio

- Somente a API .NET acessa diretamente o PostgreSQL. Frontend e n8n consomem a API.
- A API não incorpora SDKs nem regras específicas do Google Calendar. Leituras e escritas externas passam por webhooks n8n.
- O frontend não conhece URLs privadas de webhooks do n8n; fala com a API.
- Toda consulta e mutação multi-clínica respeita a clínica autenticada.
- Autorização é validada no backend. Ocultar controles no frontend não é segurança.
- Médicos/profissionais acessam somente a própria agenda. Atendentes têm leitura das agendas da clínica. Operações administrativas exigem `IsAdmin = true`.
- Telefone é a identidade de deduplicação de clientes no chatbot; normalize, aplique o escopo da clínica e trate concorrência.
- Históricos e registros de negócio não sofrem exclusão física. Cancelamento e soft delete preservam auditoria.
- Criação de profissional só persiste após criação bem-sucedida do subcalendário e recebimento do identificador externo. Trate falhas intermediárias.
- Criação de agendamento precisa ser idempotente ou protegida contra concorrência.
- Intervalos são tratados como `[início, fim)`: o fim de um agendamento pode coincidir com o início do seguinte.

## Decisões confirmadas e divergências abertas

- No desenvolvimento, use o PostgreSQL local do Compose e `database/init.sql`.
- O Supabase é somente o destino planejado para produção.
- `database/init.sql` representa sempre a criação integral de um banco vazio; não é uma migration incremental.
- O padrão de soft delete é `deleted_at IS NULL` para registro ativo. Não adicione `is_ativo` em paralelo.
- O schema atual associa cada profissional a no máximo uma especialidade por `id_especialidade`. Requisitos antigos descrevem N:N; a mudança depende de decisão explícita.
- A documentação alterna Médico/Doutor, Atendente/Recepcionista e Paciente/Cliente. Preserve os nomes atuais do código e banco até uma migração deliberada.
- Documentos antigos citam `HistoricoAtendimentos`; o modelo atual usa `agendamentos` como registro histórico de negócio.
- Bibliotecas e serviços listados como desejados não são considerados adotados até existirem no código e na configuração executável.

## Convenções de domínio, API e dados

- Preserve termos de domínio em português e a interface em `pt-BR`.
- Use data/hora com fuso explícito nas fronteiras. Persista instantes em `TIMESTAMPTZ`/ISO e formate a UI para `America/Sao_Paulo`.
- Não exponha entidades NHibernate diretamente como contratos HTTP; use DTOs.
- Valide payloads e invariantes no backend, mesmo quando o frontend já validar.
- Use `401` para não autenticado, `403` para autenticado sem escopo, `404` quando o recurso do escopo não existir e `429` para rate limit.
- Para soft delete, preencha `deleted_at` e filtre ativos com `deleted_at IS NULL`.
- Nunca faça `DELETE` físico de registros de negócio.
- Nunca versione senhas, tokens, chaves, credenciais ou URLs privadas de webhook.
- Integrações externas devem ter timeout, cancelamento, validação de respostas, retry apenas quando seguro e logs sem dados sensíveis.
- Preserve workflows n8n como JSON importável e sem credenciais exportadas.

## Frontend e design

- Siga `DESIGN.md.md` para layout, estados, RBAC visual e componentes.
- Preserve a identidade teal principal `#0097B2`, os tokens de `app/src/index.css` e ícones Lucide.
- Preserve navegação por teclado, foco visível, rótulos, contraste e estados de carregamento, erro e vazio.
- A grade principal usa FullCalendar; mantenha `timeGridWeek`, navegação semanal e filtro por profissional.
- RBAC visual melhora a experiência, mas nunca substitui autorização da API.
- Não trate os atuais dados estáticos da agenda como contrato de API.

## Fluxo de trabalho para agentes

Antes de editar:

1. Leia requisitos, regras e especificações relacionadas.
2. Verifique código, schema, Compose e workflows reais.
3. Identifique a fase PoC/MVP e os RF/RNF/RN afetados.
4. Declare divergências que possam mudar arquitetura, dados ou comportamento.
5. Verifique o `git status` e preserve alterações existentes do usuário.

Durante a implementação:

- Faça a menor alteração coerente que entregue o comportamento solicitado.
- Não antecipe itens futuros do cronograma sem necessidade.
- Mantenha entidade, mapping e `database/init.sql` sincronizados.
- Mantenha o mapa de variáveis e a árvore deste arquivo sincronizados quando estrutura/configuração mudar.
- Evite novas dependências de produção sem necessidade e compatibilidade verificadas.
- Não marque checkboxes do cronograma nem reescreva o diário sem solicitação explícita.

Ao concluir:

- Execute as verificações aplicáveis.
- Informe arquivos alterados, comportamento entregue, requisitos atendidos e limitações.
- Atualize documentação somente quando estiver no escopo ou a mudança a tiver tornado incorreta.

## Comandos e verificação

O fluxo preferido é conteinerizado. Não assuma SDKs instalados no host.

Configuração:

```powershell
docker compose config
```

Ambiente local:

```powershell
docker compose up --build
```

Frontend:

```powershell
docker compose run --rm app npm run lint
docker compose run --rm app npm run build
```

Backend:

```powershell
docker compose run --rm api dotnet build AgendAi.sln
docker compose run --rm api dotnet test AgendAi.sln
```

Equivalentes no host:

```powershell
npm --prefix app run lint
npm --prefix app run build
dotnet build api/AgendAi.sln
dotnet test api/AgendAi.sln
```

Selecione as verificações conforme o impacto:

- `app/`: lint e build do frontend.
- `api/`: build e testes .NET relevantes.
- Persistência: entidade, mapping, schema, constraints, índices e soft delete.
- Compose/Dockerfiles: `docker compose config` e, quando viável, serviços afetados.
- Workflows: JSON/importação, contratos, erros e ausência de credenciais.
- Ponta a ponta: React -> API -> n8n -> Google Calendar e n8n -> API -> PostgreSQL.

Não declare testes aprovados quando somente build ou lint foram executados. Atualmente não há suites automatizadas no repositório.

## Prioridades de revisão de código

- Isolamento entre clínicas e RBAC no backend.
- Vazamento de credenciais e URLs privadas.
- Acesso direto indevido ao banco ou ao n8n.
- Duplicidade de agendamentos, falta de idempotência e falhas parciais entre API, n8n e Google Calendar.
- Exclusão física, consultas que incluem inativos e schema sem mapping/entidade sincronizados.
- UI sem estados de permissão, carregamento, erro, vazio, acessibilidade ou responsividade.
- Diferença entre falha atual e funcionalidade apenas planejada; ausência de item futuro não é defeito por si só.
