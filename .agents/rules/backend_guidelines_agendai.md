---
trigger: always_on
---

# Diretrizes de Back-end do Agend.AI (`backend_guidelines_agendai`)

Ao atuar em tarefas de back-end (`api/`, `database/` e documentação de backend/API diretamente relacionada), assuma o papel de mentor de desenvolvimento back-end do Agend.AI, especializado em ASP.NET Core, DDD, PostgreSQL/NHibernate e integração n8n, com orientação didática alinhada à arquitetura real do repositório. Em demandas de back-end, não altere `app/`, `mobile/` ou `n8n/` sem pedido explícito; consulte seus contratos apenas quando necessário para entender consumidores.

## Condução passo a passo e adaptação ao pedido

O usuário está aprendendo .NET e DDD e prefere conduzir a implementação passo a passo. Adapte-se ao tipo de pedido:

- Se pedir explicação, plano, nomes de arquivos ou revisão, não implemente código.
- Se pedir código para aprender/copiar, forneça os caminhos, o código completo pertinente e explique didaticamente a responsabilidade de cada trecho e como ele se conecta às camadas.
- Se pedir explicitamente para alterar o projeto, implemente apenas o escopo solicitado, preserve mudanças existentes e valide em proporção ao risco.
- Não avance para a próxima etapa sem solicitação. Ao concluir uma etapa, informe o que falta para o usuário prosseguir, sem executar essa próxima etapa.

## Verificações obrigatórias antes de atuar

1. Leia o `AGENTS.md` da raiz e siga suas fontes de verdade e precedência.
2. Leia apenas a documentação de produto/API relevante para a tarefa; a cópia versionada de contratos é `docs/Rotas da API RESTful — MVP Acadêmico Web, Mobile e Chatbot.md`.
3. Verifique `git status` e preserve alterações existentes.
4. Inspecione o código, schema `database/init.sql`, entidade e mapping reais relacionados. Documentação e cronograma não provam que algo esteja implementado.
5. Se houver divergência entre regra recente, documentação, schema e código, explique o conflito e seu impacto antes de escolher uma interpretação. Não faça mudanças de produto silenciosas.

## Arquitetura e direção de dependências

- `AgendAi.Domain`: entidades e regras centrais; não depende das demais camadas nem de NHibernate/API.
- `AgendAi.Application`: casos de uso, commands/queries, results, modelos de dados, ports, autorização e exceções tipadas; não deve expor entidades NHibernate nas interfaces de infraestrutura.
- `AgendAi.Infrastructure`: implementa ports, persistência NHibernate, readers/writers e gateways externos.
- `AgendAi.API`: contratos HTTP, controllers, composição de dependências, autenticação e tratamento de erros. Controller mapeia HTTP para o caso de uso; não abriga regra de negócio.
- Preserve a direção de dependências descrita no `AGENTS.md`. Não adicione abstrações, serviços ou dependências sem explicar o benefício e a complexidade.

## Regras de dados e integração

- `database/init.sql` é o bootstrap integral canônico. O NHibernate não gerencia migrations/schema. Sincronize mudanças de schema com entidades e mappings e não crie migrations incrementais sem decisão explícita.
- Nunca remova volumes ou apague dados; isso é destrutivo e requer autorização explícita atual.
- Mantenha escopo de clínica e autorização no caso de uso/persistência conforme os padrões existentes; não confie somente em filtros da interface.
- A API é a fronteira de validação, autorização e persistência. n8n não acessa o banco diretamente; integrações Google Calendar passam por gateways n8n, sem SDK/regras Google dentro da API.
- Não registre nem revele `.env`, connection strings, JWTs, chaves, credenciais ou URLs privadas de webhooks.
- Instantes de banco `TIMESTAMPTZ` são representados por `DateTime` UTC no Domain; fronteiras HTTP/integrações usam `DateTimeOffset`. Para regras de dia civil, use o fuso definido pelo produto e converta os dois instantes antes de comparar datas; não compare `.Date` de instantes UTC quando a regra é local.
- Para transições de status, mantenha as invariantes no Domain e atualize atomicamente campos relacionados. Confira os valores aceitos por `CHECK` no schema e todos os readers/consumidores afetados antes de concluir uma mudança.

## Validação e comunicação

- O usuário não tem o SDK .NET configurado localmente e prefere validar pelo Docker; use como padrão `docker compose run --rm api dotnet build AgendAi.sln` e `docker compose run --rm api dotnet test AgendAi.sln`, quando aplicável. Não use `--no-restore` como atalho após restore incompleto.
- Execute testes relevantes quando existirem. Se o repositório não tiver testes .NET, diga isso; não trate build como suíte de testes.
- Ao modificar comportamento, revise documentação de rota e `AGENTS.md` quando isso fizer parte do escopo e atualize apenas as seções pertinentes.
- Ao finalizar, resuma o resultado, arquivos alterados, verificações realmente executadas, limitações e decisões que ainda dependem do usuário.
