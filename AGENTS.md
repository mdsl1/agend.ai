# AGENTS.md

## Propósito

Este arquivo contém instruções duráveis para agentes que trabalham no Agend.AI. Mantenha-o curto e operacional; detalhes de produto, planejamento e design pertencem à documentação do projeto.

O Agend.AI é um CRM para clínicas com agenda semanal, cadastros e automação de agendamentos. O sistema combina uma SPA React, uma API ASP.NET Core, PostgreSQL/NHibernate e workflows n8n integrados ao Telegram e ao Google Calendar.

## Fontes de verdade

A documentação de produto está atualmente fora deste repositório, na raiz de workspace:

`C:\Users\markn\mdsl1\Projetos Pessoais\Agend.AI`

Se futuramente existir `docs/` neste repositório, prefira a cópia versionada em `docs/`. Se nenhuma das duas localizações estiver disponível, informe a ausência antes de tomar uma decisão de produto.

Leia apenas os documentos pertinentes à tarefa:

- `Agend.AI.md`: visão geral, ciclo de vida e estado macro das fases.
- `Planejamento/Objetivos e Definição de Sucesso.md`: objetivos do produto e marcos de sucesso.
- `Planejamento/Requisitos.md`: requisitos funcionais e não funcionais, identificados por RF/RNF.
- `Planejamento/Regras de Negócio.md`: invariantes de integração, RBAC, agendamento e retenção de dados, identificadas por RN.
- `Planejamento/Stack.md`: stack desejada; confirme no código se cada dependência já foi adotada.
- `Planejamento/Etapas do Projeto e Cronograma.md`: sequência planejada de PoC e MVP; não trate estimativas como estado implementado.
- `Diário de Bordo.md`: registro histórico do trabalho concluído.
- `Design - Arquitetura - Infraestrutura/DESIGN.md.md`: comportamento visual e especificações das telas.
- `Design - Arquitetura - Infraestrutura/Estrutura de Tabelas.md`: modelo conceitual; valide sempre contra `database/init.sql`, entidades e mapeamentos.

Use esta precedência:

1. A solicitação atual do usuário define o escopo.
2. Regras de negócio e requisitos definem o comportamento desejado.
3. Decisões explícitas mais recentes do usuário prevalecem sobre planos antigos.
4. O código, o schema e os workflows representam o estado realmente implementado.
5. Cronograma e diário de bordo fornecem contexto, mas não substituem verificação no código.

Quando documentação e implementação divergirem, descreva a divergência e seu impacto. Não altere silenciosamente o comportamento para fazer um lado coincidir com o outro. Para decisões significativas, peça direção ou registre claramente a hipótese adotada.

## Estado e estrutura do repositório

- `app/`: React 19, TypeScript e Vite. No estado atual, ainda está próximo do scaffold inicial.
- `api/AgendAi.Domain/`: entidades e regras centrais do domínio.
- `api/AgendAi.Application/`: casos de uso, contratos, validação e mapeamento de aplicação.
- `api/AgendAi.Infrastructure/`: NHibernate, mapeamentos e integrações de infraestrutura.
- `api/AgendAi.API/`: composição, controllers, autenticação, autorização e transporte HTTP.
- `database/init.sql`: schema e dados iniciais do PostgreSQL local.
- `n8n/workflows/`: workflows exportáveis do n8n.
- `docker-compose.yml`: ambiente local com app, API, PostgreSQL e n8n.

Não presuma que itens descritos para o MVP já existem. Antes de modificar uma área, inspecione sua implementação e procure testes relacionados.

## Invariantes arquiteturais

- Somente a API .NET pode acessar diretamente o PostgreSQL. Frontend e n8n devem consumir a API.
- A API não deve incorporar SDKs nem regras específicas do Google Calendar. Leituras e escritas de agenda passam por webhooks do n8n.
- O frontend não deve conhecer URLs privadas de webhooks do n8n; deve falar com a API.
- Preserve a direção das dependências: Domain não depende de Application, Infrastructure ou API; Application depende de Domain; Infrastructure implementa persistência e integrações; API compõe as camadas.
- Regras de negócio não pertencem a controllers nem a componentes visuais.
- Toda consulta e mutação de dados multi-clínica deve respeitar o escopo da clínica autenticada.
- A autorização deve ser validada no backend. Ocultar controles no frontend não é mecanismo de segurança.
- Médicos só podem acessar a própria agenda. Atendentes têm leitura das agendas da clínica. Operações administrativas exigem `IsAdmin = true`.
- Telefone é a identidade de deduplicação de pacientes no fluxo do chatbot; valide o escopo da clínica e a concorrência antes de criar um novo registro.
- Históricos e registros de negócio não devem sofrer exclusão física. Preserve auditoria por cancelamento ou soft delete com `deleted_at`.
- A criação de médico só deve persistir após a criação bem-sucedida do subcalendário e o recebimento do identificador externo. Trate falhas intermediárias explicitamente.
- A criação de agendamento precisa ser idempotente ou protegida contra concorrência para impedir reservas duplicadas.

## Decisões confirmadas

- Durante o desenvolvimento, use o PostgreSQL local definido em `docker-compose.yml` e inicializado por `database/init.sql`.
- O Supabase é o destino planejado para o PostgreSQL de produção. Preserve compatibilidade com PostgreSQL e mantenha a conexão configurável por ambiente para permitir essa migração sem alterar regras de negócio.
- O padrão canônico de soft delete é o implementado no código e no schema atual: `deleted_at IS NULL` representa registro ativo; excluir significa preencher `deleted_at` com timestamp. Não adicione `is_ativo` como mecanismo paralelo sem nova decisão explícita.

## Divergências conhecidas

Considere estes pontos em aberto ao trabalhar nas áreas afetadas:

- Os requisitos descrevem relação N:N entre profissionais e especialidades, enquanto o schema atual contém um único `id_especialidade` em `profissionais`. Confirme o modelo antes de ampliar essa funcionalidade.
- A documentação usa termos como Médico/Doutor, Atendente/Recepcionista e Paciente/Cliente. Preserve os nomes atuais em código e banco até que uma migração de nomenclatura seja deliberadamente aprovada.
- A stack documentada contém bibliotecas ainda ausentes do frontend e recursos de segurança ainda não configurados na API. Não escreva código supondo que já estejam disponíveis.

## Fluxo de trabalho

Antes de editar:

1. Leia os requisitos, regras e especificações relacionadas à tarefa.
2. Verifique o estado real nos arquivos de código, schema, Compose e workflows.
3. Identifique a fase PoC/MVP e os identificadores RF, RNF ou RN afetados.
4. Declare qualquer divergência que possa mudar arquitetura, dados ou comportamento.

Durante a implementação:

- Faça a menor alteração coerente que entregue o comportamento solicitado.
- Não implemente antecipadamente itens futuros do cronograma sem necessidade para a tarefa atual.
- Mantenha entidades, mapeamentos NHibernate e `database/init.sql` sincronizados quando o modelo persistido mudar.
- Trate integrações externas com timeout, cancelamento, respostas inválidas, retry apenas quando seguro e logs sem dados sensíveis.
- Evite adicionar dependências de produção sem necessidade clara e sem verificar compatibilidade com a stack existente.
- Preserve workflows n8n em formato importável e nunca versione credenciais exportadas.
- Não marque checkboxes do cronograma nem reescreva o diário de bordo sem solicitação explícita.

Ao concluir:

- Execute as verificações aplicáveis.
- Informe arquivos alterados, comportamento entregue, requisitos atendidos e limitações restantes.
- Atualize documentação somente quando estiver no escopo ou quando a mudança tornar a documentação incorreta; mantenha alterações documentais pontuais.

## Convenções de domínio, API e dados

- Preserve os termos de domínio em português e a interface em `pt-BR`.
- Use tipos de data/hora com fuso explícito nas fronteiras. Persista instantes em `TIMESTAMPTZ`/formato ISO e formate a UI para `America/Sao_Paulo`.
- Não exponha entidades NHibernate diretamente como contratos HTTP; use DTOs quando endpoints forem implementados.
- Valide payloads e invariantes no backend, inclusive quando o frontend já fizer validação.
- Use respostas HTTP coerentes: `401` para não autenticado, `403` para autenticado sem escopo, `404` quando o recurso do escopo não existir e `429` para rate limit.
- Não faça `DELETE` físico de dados de negócio. Para soft delete, preencha `deleted_at` e filtre registros ativos com `deleted_at IS NULL`. Operações destrutivas de banco, volumes ou migrações exigem autorização explícita.
- Nunca adicione senhas, tokens, chaves, credenciais do Supabase, Telegram, Google, Groq ou URLs privadas de webhook ao Git. Use variáveis de ambiente e exemplos sem valores reais.

## Frontend e design

- Siga `DESIGN.md.md` para layout, estados, RBAC visual e componentes.
- Use a identidade principal teal `#0097B2`, os tokens documentados e ícones `lucide-react` quando essas dependências estiverem instaladas.
- Preserve acessibilidade: navegação por teclado, foco visível, rótulos de formulário, contraste e estados de carregamento/erro/vazio.
- A grade principal planejada usa FullCalendar em `timeGridWeek`, navegação semanal e filtro por profissional.
- RBAC visual melhora a experiência, mas nunca substitui autorização da API.

## Comandos e verificação

O fluxo preferido é conteinerizado. Não assuma que SDKs estejam instalados no host.

Validação da configuração:

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

Se o host já possuir as ferramentas, os equivalentes são:

```powershell
npm --prefix app run lint
npm --prefix app run build
dotnet build api/AgendAi.sln
dotnet test api/AgendAi.sln
```

Selecione as verificações conforme o impacto:

- Alterações em `app/`: lint e build do frontend.
- Alterações em `api/`: build e testes .NET relevantes.
- Alterações de persistência: conferir entidade, mapping, schema, constraints, índices e comportamento de soft delete.
- Alterações em Compose/Dockerfiles: `docker compose config` e, quando viável, subir os serviços afetados.
- Alterações em workflows: validar JSON/importação, contratos de webhook, caminhos de erro e ausência de credenciais.
- Alterações ponta a ponta: verificar o caminho React -> API -> n8n -> Google Calendar e, quando houver persistência, n8n -> API -> PostgreSQL.

Atualmente não há projetos de teste nem testes frontend no repositório. Não declare que testes passaram quando apenas build ou lint foram executados; registre verificações ausentes de forma explícita.

## Revisão de código

- Priorize violações de isolamento entre clínica, RBAC, vazamento de credenciais e acesso direto indevido ao banco ou ao n8n.
- Procure duplicidade de agendamentos, operações não idempotentes e falhas parciais entre API, n8n e Google Calendar.
- Sinalize exclusão física, consultas que incluem registros inativos e alterações de schema sem atualização dos mapeamentos.
- Verifique que mudanças de UI respeitam estados de permissão, carregamento, erro, vazio e responsividade.
- Diferencie falhas atuais de funcionalidades apenas planejadas; ausência de item futuro não é defeito por si só.
