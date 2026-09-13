# Rotas da API RESTful — MVP Acadêmico Web, Mobile e Chatbot

> Última revisão: 13 de setembro de 2026.

Este documento define os contratos HTTP necessários para integrar os três projetos:

- **Web:** CRM React utilizado pela clínica;
- **Mobile:** aplicativo **Agend.AI Profissional**, desenvolvido com React Native, Expo e TypeScript;
- **Chatbot:** workflow n8n que atende o cliente pelo Telegram.

## Estado atual

As seguintes rotas já existem na PoC:

- `GET /api/agenda/{profissionalUuid}?inicio={iso}&fim={iso}`;
- `GET /api/profissionais?clinicaUuid={clinicaUuid}&especialidadeUuid={uuid-opcional}`;
- `GET /api/profissionais/{profissionalUuid}/disponibilidade?profissionalProcedimentoUuid={uuid}&inicio={iso}&limite={1-3}`;
- `POST /api/clientes/resolver?clinicaUuid={clinicaUuid}`;
- `POST /api/agendamentos`.

Elas ainda não possuem autenticação. Os demais contratos estão planejados e todos devem respeitar o escopo da clínica e o RBAC. Na PoC, a criação já rejeita cliente e associação profissional/procedimento pertencentes a clínicas diferentes.

## Convenções gerais

### Autenticação e escopo

- Web e Mobile usam `Authorization: Bearer <access-token>`.
- O login retorna somente um access token JWT, com duração sugerida de 2 a 4 horas.
- O Mobile armazena o token no Expo SecureStore. O Web pode mantê-lo em memória ou `sessionStorage` durante o MVP.
- O logout é local: o cliente remove o token e o estado da sessão. Não existe rota de logout neste contrato.
- O chatbot usa uma credencial de serviço exclusiva para o sentido **n8n → API**.
- Nas rotas autenticadas, a clínica é obtida do token. O cliente não envia `idClinica`.
- O profissional acessa somente os próprios dados. A API deve retornar `403` ao tentar acessar recursos de outro profissional.
- `isAdmin = true` é a fonte de autoridade administrativa; o campo `cargo` não concede permissão sozinho.

### Datas, identificadores e exclusões

- Datas e horas usam ISO 8601 com offset, como `2026-09-10T09:00:00-03:00`.
- Intervalos seguem `[inicio, fim)`: o fim de um evento pode coincidir com o início do próximo.
- Recursos persistidos no PostgreSQL são expostos por UUID, nunca pelo ID interno `BIGINT`.
- IDs de eventos do Calendar são tratados pelo cliente como strings opacas. Antes de alterar ou excluir um evento, a API deve validar o calendário, o profissional e o tipo do evento.
- Registros de negócio não sofrem exclusão física. Cancelamentos e inativações preservam o histórico.
- Consultas de agenda aceitam períodos de no máximo 45 dias. A disponibilidade consulta um horário específico e, quando necessário, procura alternativas nos sete dias posteriores ao término solicitado.

### O que é idempotência?

Idempotência significa que repetir a mesma operação produz o mesmo resultado, sem duplicar seus efeitos. Ela é especialmente importante quando uma requisição pode ser reenviada após timeout, falha de rede ou retentativa do n8n.

Em uma criação de agendamento, o consumidor envia uma chave exclusiva para aquela ação:

```http
POST /api/agendamentos
Idempotency-Key: 8ab27468-482f-46fb-8c4c-8acf90e773a1
```

Na primeira chamada, a API cria o recurso e associa a resposta à chave. Se a mesma requisição for repetida com a mesma chave, a API devolve o resultado original em vez de criar outro agendamento. A mesma chave com um corpo diferente deve retornar `409 Conflict`.

O Web ou o chatbot gera uma chave aleatória para cada nova intenção de agendamento e conserva essa mesma chave enquanto repetir a mesma requisição. Uma nova confirmação deve usar outra chave. A chave não é um segredo nem uma credencial de autenticação: ela serve apenas para correlacionar retentativas, tem limite de 200 caracteres e seu escopo é a clínica.

Neste contrato, `Idempotency-Key` é obrigatória somente em `POST /api/agendamentos`, porque essa operação atravessa API, banco, n8n e Google Calendar. As demais mutações usam validações de estado, bloqueio de botão no cliente e constraints do banco.

### Listagens

As listagens deste MVP não utilizam busca nem paginação. Elas retornam somente registros ativos da clínica e são adequadas ao volume controlado da apresentação.

### Erros

As falhas usam `application/problem+json`:

```json
{
  "type": "https://api.agend.ai/problems/recurso-nao-encontrado",
  "title": "Recurso não encontrado",
  "status": 404,
  "detail": "O agendamento informado não existe neste escopo.",
  "codigo": "agendamento_nao_encontrado",
  "traceId": "00-a1b2c3"
}
```

Códigos usuais: `400` para entrada inválida, `401` para não autenticado, `403` para falta de permissão, `404` para recurso inexistente, `409` para conflito, `502` para falha de integração e `500` para erro inesperado.

---

## 1. Autenticação e sessão

### `POST /api/auth/login`

- **Projeto:** Web e Mobile.
- **Estado/acesso:** planejada; pública.
- **Descrição:** valida clínica, e-mail e senha, e retorna um access token com o contexto do usuário.

Entrada:

```json
{
  "clinicaUuid": "20d84992-1f87-42cc-a251-92cac362f41f",
  "email": "marina.souza@clinicavida.com",
  "senha": "senha-forte"
}
```

Saída `200 OK`:

```json
{
  "accessToken": "jwt-de-exemplo",
  "expiraEm": "2026-09-02T16:00:00-03:00",
  "usuario": {
    "uuid": "36629372-b7fe-4388-80f0-a6e879aad21f",
    "nome": "Marina Souza",
    "cargo": "Profissional",
    "isAdmin": false,
    "profissionalUuid": "abf43724-5590-4475-a11f-b679e285477d",
    "clinicaUuid": "20d84992-1f87-42cc-a251-92cac362f41f"
  }
}
```

Credenciais inválidas retornam uma mensagem genérica com `401`, sem revelar qual campo está incorreto.

### `GET /api/me`

- **Projeto:** Web e Mobile.
- **Estado/acesso:** planejada; usuário autenticado.
- **Descrição:** valida o token salvo e recupera o perfil atual, o vínculo com a clínica e o contexto de RBAC. Ao receber `401`, o cliente remove o token e volta ao login.

Entrada:

```http
GET /api/me
Authorization: Bearer <access-token>
```

Saída `200 OK`:

```json
{
  "usuarioUuid": "36629372-b7fe-4388-80f0-a6e879aad21f",
  "profissionalUuid": "abf43724-5590-4475-a11f-b679e285477d",
  "nome": "Marina Souza",
  "prefixo": "Dra.",
  "email": "marina.souza@clinicavida.com",
  "cargo": "Profissional",
  "isAdmin": false,
  "registroProfissional": "CRM 12345",
  "especialidade": "Dermatologia",
  "clinica": {
    "uuid": "20d84992-1f87-42cc-a251-92cac362f41f",
    "nome": "Clínica Vida"
  },
  "permissoes": ["agenda:propria", "procedimentos:proprios", "receitas:proprias"]
}
```

---

## 2. Agenda, disponibilidade e agendamentos

### `GET /api/agenda/{profissionalUuid}`

- **Projeto:** Web e Mobile.
- **Estado/acesso:** implementada na PoC sem autenticação; deverá aplicar RBAC.
- **Descrição:** consulta, em uma única resposta, os agendamentos e as indisponibilidades existentes no Google Calendar do profissional.

Entrada:

```http
GET /api/agenda/abf43724-5590-4475-a11f-b679e285477d?inicio=2026-09-01T00:00:00-03:00&fim=2026-09-08T00:00:00-03:00
Authorization: Bearer <access-token>
```

Saída `200 OK`:

```json
{
  "eventos": [
    {
      "id": "evento-opaco-123",
      "tipo": "agendamento",
      "agendamentoUuid": "a3711381-e0f5-4ddd-828d-a35f45797ca4",
      "titulo": "Consulta",
      "inicio": "2026-09-02T09:00:00-03:00",
      "fim": "2026-09-02T09:45:00-03:00",
      "nomeCliente": "João Silva",
      "nomeProcedimento": "Consulta inicial",
      "profissionalUuid": "abf43724-5590-4475-a11f-b679e285477d"
    },
    {
      "id": "evento-opaco-456",
      "tipo": "indisponibilidade",
      "agendamentoUuid": null,
      "titulo": "Indisponível",
      "inicio": "2026-09-02T12:00:00-03:00",
      "fim": "2026-09-02T13:00:00-03:00",
      "nomeCliente": null,
      "nomeProcedimento": null,
      "profissionalUuid": "abf43724-5590-4475-a11f-b679e285477d"
    }
  ]
}
```

O campo `tipo` define o componente visual usado pelo Web e pelo Mobile. Um profissional só pode consultar seu próprio UUID.

### `GET /api/profissionais/{profissionalUuid}/disponibilidade`

- **Projeto:** Chatbot.
- **Estado/acesso:** implementada na PoC sem autenticação; deverá aceitar somente a identidade de serviço do chatbot.
- **Descrição:** responde se um horário específico está disponível e, quando estiver ocupado, retorna até três alternativas próximas. Considera a duração efetiva do procedimento, as janelas semanais da clínica e os eventos existentes no Google Calendar. Web e Mobile não usam esta rota.

Entrada:

```http
GET /api/profissionais/abf43724-5590-4475-a11f-b679e285477d/disponibilidade?profissionalProcedimentoUuid=9528014c-5161-4e5e-a8a1-ebf1c26f7417&inicio=2026-09-03T09:00:00-03:00&limite=3
```

Parâmetros:

- `profissionalProcedimentoUuid` é obrigatório e identifica a associação ativa entre o profissional e o procedimento;
- `inicio` é obrigatório e deve ser enviado como ISO 8601 com offset explícito;
- `limite` é opcional, aceita valores de 1 a 3 e possui valor padrão 3;
- `fim` não é recebido do cliente: a API consulta a duração efetiva do procedimento e calcula o término.

Saída `200 OK` quando o horário solicitado está disponível:

```json
{
  "disponivel": true,
  "duracaoMinutos": 45,
  "horariosDisponiveis": [
    {
      "inicio": "2026-09-03T09:00:00-03:00",
      "fim": "2026-09-03T09:45:00-03:00"
    }
  ],
  "buscaEsgotada": false
}
```

Saída `200 OK` quando o horário solicitado está ocupado:

```json
{
  "disponivel": false,
  "duracaoMinutos": 45,
  "horariosDisponiveis": [
    {
      "inicio": "2026-09-03T09:45:00-03:00",
      "fim": "2026-09-03T10:30:00-03:00"
    },
    {
      "inicio": "2026-09-03T10:30:00-03:00",
      "fim": "2026-09-03T11:15:00-03:00"
    },
    {
      "inicio": "2026-09-03T11:15:00-03:00",
      "fim": "2026-09-03T12:00:00-03:00"
    }
  ],
  "buscaEsgotada": false
}
```

A API obtém internamente a duração, as janelas de atendimento, o ID privado do calendário e a URL privada do webhook. O período calculado é validado em `America/Sao_Paulo` antes da integração; se estiver fora do atendimento ou atravessar a data local, a solicitação termina em `400` sem chamar o n8n. Para um horário ocupado, o workflow procura alternativas a partir do fim solicitado por até sete dias, mantendo a mesma duração.

O contrato privado da chamada API → workflow de agenda não é devolvido ao fluxo conversacional. A resposta do workflow precisa correlacionar `profissional_uuid` e `periodo_solicitado`, além de informar `sucesso`, `disponivel`, `alternativas_disponiveis` e `busca_esgotada`. A API valida duração, intervalo de busca, duplicidades e janelas de atendimento antes de montar a resposta consumida pelo chatbot.

Falhas esperadas:

- `400 Bad Request`: UUID, início ou limite inválido, ou período fora da janela de atendimento;
- `404 Not Found`: associação ativa entre profissional e procedimento não encontrada;
- `409 Conflict`: duração, agenda externa, janelas ou webhook não configurados;
- `502 Bad Gateway`: timeout, erro HTTP, JSON inválido ou resposta incoerente do n8n.

Essa resposta é apenas uma fotografia. A criação deve validar novamente se o horário continua livre.

### `GET /api/agendamentos/{agendamentoUuid}`

- **Projeto:** Web e Mobile.
- **Estado/acesso:** planejada; profissional responsável ou usuário autorizado da clínica.
- **Descrição:** retorna os dados necessários para a tela de detalhes de um atendimento.

Entrada:

```http
GET /api/agendamentos/a3711381-e0f5-4ddd-828d-a35f45797ca4
Authorization: Bearer <access-token>
```

Saída `200 OK`:

```json
{
  "uuid": "a3711381-e0f5-4ddd-828d-a35f45797ca4",
  "cliente": {
    "uuid": "438d71b4-197c-44d9-bf1e-e6e84e983e67",
    "nome": "João Silva",
    "telefone": "+5511999991234"
  },
  "profissional": {
    "uuid": "abf43724-5590-4475-a11f-b679e285477d",
    "nomeExibicao": "Dra. Ana Lima"
  },
  "procedimento": {
    "uuid": "19ad57ea-7523-4659-8eab-6d1475831a13",
    "nome": "Consulta inicial",
    "duracaoMinutos": 45
  },
  "inicio": "2026-09-10T15:00:00-03:00",
  "fim": "2026-09-10T15:45:00-03:00",
  "motivoContato": "Retorno",
  "anotacoesProfissional": null,
  "valorTotal": 220.00,
  "status": "agendado"
}
```

### `POST /api/agendamentos`

- **Projeto:** Web e Chatbot.
- **Estado/acesso:** implementada na PoC; ainda sem autenticação.
- **Descrição:** revalida o horário, reserva o intervalo no PostgreSQL e cria o evento correspondente no Google Calendar por meio do n8n. A API deriva profissional, procedimento, duração, fim e valor da associação `profissional_procedimentos`.

Entrada:

```http
POST /api/agendamentos
Idempotency-Key: 8ab27468-482f-46fb-8c4c-8acf90e773a1
Content-Type: application/json
```

```json
{
  "clienteUuid": "438d71b4-197c-44d9-bf1e-e6e84e983e67",
  "profissionalProcedimentoUuid": "9528014c-5161-4e5e-a8a1-ebf1c26f7417",
  "inicio": "2026-09-10T15:00:00-03:00",
  "motivoContato": "Manchas e coceira no braço"
}
```

Saída `201 Created`:

```json
{
  "uuid": "a3711381-e0f5-4ddd-828d-a35f45797ca4",
  "inicio": "2026-09-10T18:00:00+00:00",
  "fim": "2026-09-10T18:45:00+00:00",
  "nomeCliente": "João Silva",
  "nomeProfissional": "Dra. Ana Lima",
  "nomeProcedimento": "Consulta inicial",
  "valorTotal": 220.00,
  "status": "agendado"
}
```

Os instantes da resposta podem ser normalizados para UTC; no exemplo, `18:00+00:00` representa o mesmo instante de `15:00-03:00` enviado na requisição.

Fluxo confirmado na PoC:

1. O controller exige `Idempotency-Key` e transforma o header e o corpo em comando.
2. O caso de uso valida a entrada e carrega cliente, associação ativa, profissional, procedimento, clínica, duração, valor, agenda externa e janelas de atendimento.
3. A chave é pesquisada dentro da clínica. Uma repetição idêntica já concluída retorna novamente `201` com o mesmo UUID, sem consultar disponibilidade nem criar outro evento. A mesma chave com corpo diferente, ainda pendente ou já marcada como falha retorna `409`.
4. O serviço compartilhado de disponibilidade valida o intervalo e consulta o n8n. A rota não chama internamente outro controller ou handler HTTP.
5. O agendamento é persistido como `pendente_integracao`; a constraint de sobreposição do banco é a barreira final contra concorrência.
6. A API solicita ao n8n a criação de um evento com ID determinístico derivado do UUID do agendamento. Há timeout de 10 segundos e, para falhas transitórias, no máximo duas tentativas totais.
7. Em sucesso, a API registra o ID externo e altera o status para `agendado`. Em falha externa, persiste `falha_integracao` e retorna `502`.

Falhas esperadas:

- `400`: entrada inválida, chave ausente/inválida ou intervalo fora da janela de atendimento;
- `404`: cliente ou associação profissional/procedimento inexistente, inativa ou fora do mesmo escopo de clínica;
- `409`: horário indisponível, configuração ausente, reutilização incompatível da chave, operação pendente ou falha, ou conflito concorrente protegido pelo banco;
- `502`: timeout, falha HTTP ou resposta inválida do n8n;
- `500`: erro inesperado.

A rota foi validada manualmente nos cenários de criação, repetição idempotente, reutilização da chave com outro corpo, chave ausente ou inválida, recursos inexistentes, período fora do atendimento, horário ocupado, concorrência, falha externa e nova tentativa com outra chave após correção da integração.

### `PATCH /api/agendamentos/{agendamentoUuid}/status`

- **Projeto:** Web.
- **Estado/acesso:** planejada; usuário autorizado da clínica.
- **Descrição:** altera o agendamento para `concluido`, `cancelado` ou `faltou`, preservando o histórico. Ao cancelar, também remove ou cancela o evento correspondente no Calendar.

Entrada:

```json
{
  "status": "cancelado",
  "motivo": "Solicitação do cliente"
}
```

Saída `200 OK`:

```json
{
  "uuid": "a3711381-e0f5-4ddd-828d-a35f45797ca4",
  "status": "cancelado",
  "atualizadoEm": "2026-09-02T18:42:10-03:00"
}
```

Uma transição inválida, como cancelar um atendimento já concluído, retorna `409`.

### `POST /api/me/indisponibilidades`

- **Projeto:** Mobile.
- **Estado/acesso:** planejada; profissional autenticado.
- **Descrição:** cria um evento pontual e ocupado no Google Calendar do próprio profissional. O título é sempre definido pelo servidor como `Indisponível`.

Entrada:

```json
{
  "inicio": "2026-09-10T12:00:00-03:00",
  "fim": "2026-09-10T13:00:00-03:00"
}
```

Saída `201 Created`:

```json
{
  "id": "evento-opaco-456",
  "tipo": "indisponibilidade",
  "titulo": "Indisponível",
  "inicio": "2026-09-10T12:00:00-03:00",
  "fim": "2026-09-10T13:00:00-03:00"
}
```

Não há recorrência nem persistência no PostgreSQL. Intervalo inválido ou sobreposto a outro evento retorna `409`.

### `DELETE /api/me/indisponibilidades/{eventoId}`

- **Projeto:** Mobile.
- **Estado/acesso:** planejada; profissional autenticado.
- **Descrição:** remove um evento de indisponibilidade para liberar novamente o período. A API deve confirmar que o evento pertence à agenda do profissional e foi criado como indisponibilidade.

Entrada:

```http
DELETE /api/me/indisponibilidades/evento-opaco-456
Authorization: Bearer <access-token>
```

Saída: `204 No Content`.

Eventos de agendamento não podem ser removidos por esta rota.

---

## 3. Clientes

### `GET /api/clientes`

- **Projeto:** Web.
- **Estado/acesso:** planejada; usuário autorizado da clínica.
- **Descrição:** lista todos os clientes ativos da clínica, ordenados por nome, sem busca e sem paginação.

Entrada:

```http
GET /api/clientes
Authorization: Bearer <access-token>
```

Saída `200 OK`:

```json
{
  "clientes": [
    {
      "uuid": "438d71b4-197c-44d9-bf1e-e6e84e983e67",
      "nome": "João Silva",
      "telefone": "+5511999991234",
      "email": "joao@email.com"
    }
  ]
}
```

### `POST /api/clientes`

- **Projeto:** Web.
- **Estado/acesso:** planejada; usuário autorizado da clínica.
- **Descrição:** cadastra manualmente um cliente. Telefone ativo duplicado na mesma clínica retorna `409`.

Entrada:

```json
{
  "nome": "João Silva",
  "cpf": "123.456.789-00",
  "email": "joao@email.com",
  "telefone": "+55 (11) 99999-1234",
  "dataNascimento": "1990-05-10",
  "genero": "masculino",
  "observacoesAnamnese": "Alergia informada pelo cliente."
}
```

Saída `201 Created`:

```json
{
  "uuid": "438d71b4-197c-44d9-bf1e-e6e84e983e67",
  "nome": "João Silva",
  "telefone": "+5511999991234",
  "ativo": true
}
```

### `GET /api/clientes/{clienteUuid}`

- **Projeto:** Web.
- **Estado/acesso:** planejada; usuário autorizado da clínica.
- **Descrição:** retorna a ficha do cliente utilizada pelo link presente no evento do Google Calendar.

Entrada:

```http
GET /api/clientes/438d71b4-197c-44d9-bf1e-e6e84e983e67
Authorization: Bearer <access-token>
```

Saída `200 OK`:

```json
{
  "uuid": "438d71b4-197c-44d9-bf1e-e6e84e983e67",
  "nome": "João Silva",
  "cpf": "123.456.789-00",
  "email": "joao@email.com",
  "telefone": "+5511999991234",
  "dataNascimento": "1990-05-10",
  "genero": "masculino",
  "observacoesAnamnese": "Alergia informada pelo cliente.",
  "totalAgendamentos": 5,
  "ultimoAtendimentoEm": "2026-08-10T09:00:00-03:00"
}
```

### `POST /api/clientes/resolver`

- **Projeto:** Web e Chatbot.
- **Estado/acesso:** implementada na PoC sem autenticação; deverá aceitar usuário autorizado ou identidade de serviço e obter a clínica desse contexto.
- **Descrição:** procura atomicamente um cliente ativo pelo telefone normalizado dentro da clínica. Se não existir, cria; se existir, reutiliza o cadastro e vincula o identificador do Telegram quando permitido.

Entrada atual da PoC:

```http
POST /api/clientes/resolver?clinicaUuid=20d84992-1f87-42cc-a251-92cac362f41f
Content-Type: application/json
```

Entrada desejada:

```http
POST /api/clientes/resolver
Authorization: Bearer <access-token-ou-token-de-servico>
```

```json
{
  "nome": "Mariana Souza",
  "telefone": "+55 (11) 99999-1234",
  "telegramUserId": "583920174"
}
```

Saída `201 Created` para novo cliente ou `200 OK` para existente:

```json
{
  "clienteUuid": "438d71b4-197c-44d9-bf1e-e6e84e983e67",
  "criado": true,
  "nome": "Mariana Souza",
  "telefone": "5511999991234"
}
```

Regras implementadas:

- `nome` é obrigatório e aceita no máximo 150 caracteres;
- o telefone é reduzido a dígitos ASCII e deve resultar em 10 a 15 dígitos;
- `telegramUserId` é opcional, exclusivamente numérico e aceita no máximo 30 caracteres;
- um cliente encontrado pelo telefone é reutilizado sem sobrescrever o nome já persistido;
- o Telegram é vinculado somente quando ainda estiver vazio ou já corresponder ao mesmo identificador;
- telefone e Telegram que apontem para clientes distintos resultam em `409 Conflict`;
- clientes em soft delete não são considerados;
- telefone e Telegram ativos são protegidos por unicidade dentro da clínica, inclusive sob concorrência.

Falhas tratadas: `400 Bad Request` para entrada inválida, `404 Not Found` para clínica inexistente ou inativa, `409 Conflict` para identidade divergente ou colisão concorrente e `500 Internal Server Error` para falha inesperada. Os cenários de criação, reutilização por telefone, vínculo e repetição do Telegram, conflitos, validações e concorrência foram testados manualmente.

---

## 4. Especialidades, profissionais e procedimentos

### `GET /api/especialidades`

- **Projeto:** Chatbot.
- **Estado/acesso:** planejada; identidade de serviço do n8n.
- **Descrição:** lista todas as especialidades ativas da clínica para a etapa de seleção do chatbot.

Entrada:

```http
GET /api/especialidades
Authorization: Bearer <token-de-servico>
X-AgendAi-Clinic-Uuid: 20d84992-1f87-42cc-a251-92cac362f41f
```

Saída `200 OK`:

```json
{
  "especialidades": [
    {
      "uuid": "443445f5-3922-4238-b83e-ef8b083110d2",
      "nome": "Dermatologia"
    },
    {
      "uuid": "63f5100a-bf70-4107-a64f-2d27b917646f",
      "nome": "Cardiologia"
    }
  ]
}
```

### `GET /api/profissionais`

- **Projeto:** Web e Chatbot.
- **Estado/acesso:** implementada na PoC sem autenticação; deverá aceitar usuário autorizado ou identidade de serviço e obter a clínica desse contexto.
- **Descrição:** lista os profissionais agendáveis da clínica que possuem agenda configurada, com filtro opcional por especialidade ativa.

Entrada atual da PoC:

```http
GET /api/profissionais?clinicaUuid=20d84992-1f87-42cc-a251-92cac362f41f&especialidadeUuid=443445f5-3922-4238-b83e-ef8b083110d2
```

Entrada desejada:

```http
GET /api/profissionais?especialidadeUuid=443445f5-3922-4238-b83e-ef8b083110d2
Authorization: Bearer <access-token-ou-token-de-servico>
```

Saída `200 OK`:

```json
{
  "profissionais": [
    {
      "profissionalUuid": "abf43724-5590-4475-a11f-b679e285477d",
      "nomeExibicao": "Dra. Ana Lima",
      "especialidade": {
        "uuid": "443445f5-3922-4238-b83e-ef8b083110d2",
        "nome": "Dermatologia"
      }
    }
  ]
}
```

`especialidadeUuid` é opcional. Sem o filtro, a rota retorna todos os profissionais agendáveis da clínica; `especialidade` pode ser `null` quando o profissional não possuir uma especialidade ativa. O ID externo do Google Calendar é usado apenas como filtro interno de elegibilidade e nunca é exposto. Clínica ou especialidade sem profissionais correspondentes resulta em `200 OK` com coleção vazia; UUIDs vazios resultam em `400 Bad Request`.

### `GET /api/profissionais/{profissionalUuid}/procedimentos`

- **Projeto:** Web e Chatbot.
- **Estado/acesso:** planejada; usuário autorizado ou identidade de serviço.
- **Descrição:** lista somente os procedimentos oferecidos pelo profissional, com preço e duração efetivos.

Entrada:

```http
GET /api/profissionais/abf43724-5590-4475-a11f-b679e285477d/procedimentos
```

Saída `200 OK`:

```json
{
  "procedimentos": [
    {
      "profissionalProcedimentoUuid": "9528014c-5161-4e5e-a8a1-ebf1c26f7417",
      "procedimentoUuid": "19ad57ea-7523-4659-8eab-6d1475831a13",
      "nome": "Consulta inicial",
      "valorEfetivo": 220.00,
      "duracaoEfetivaMinutos": 45
    }
  ]
}
```

O `profissionalProcedimentoUuid` deve ser enviado ao criar um agendamento, garantindo que preço e duração pertencem àquele profissional.

### `GET /api/procedimentos`

- **Projeto:** Mobile.
- **Estado/acesso:** planejada; profissional autenticado.
- **Descrição:** lista, sem busca ou paginação, todos os procedimentos ativos do catálogo da clínica para que o profissional escolha quais deseja oferecer.

Entrada:

```http
GET /api/procedimentos
Authorization: Bearer <access-token>
```

Saída `200 OK`:

```json
{
  "procedimentos": [
    {
      "uuid": "19ad57ea-7523-4659-8eab-6d1475831a13",
      "nome": "Consulta inicial",
      "valorBase": 180.00,
      "duracaoBaseMinutos": 30,
      "oferecidoPeloProfissional": true
    },
    {
      "uuid": "81c3e389-738f-4d46-a992-11f78cb9a3fd",
      "nome": "Peeling químico",
      "valorBase": 300.00,
      "duracaoBaseMinutos": 60,
      "oferecidoPeloProfissional": false
    }
  ]
}
```

### `GET /api/me/procedimentos`

- **Projeto:** Mobile.
- **Estado/acesso:** planejada; profissional autenticado.
- **Descrição:** lista os procedimentos atualmente associados ao profissional, incluindo valores-base, personalizados e efetivos.

Entrada:

```http
GET /api/me/procedimentos
Authorization: Bearer <access-token>
```

Saída `200 OK`:

```json
{
  "procedimentos": [
    {
      "profissionalProcedimentoUuid": "9528014c-5161-4e5e-a8a1-ebf1c26f7417",
      "procedimentoUuid": "19ad57ea-7523-4659-8eab-6d1475831a13",
      "nome": "Consulta inicial",
      "valorBase": 180.00,
      "valorPersonalizado": 220.00,
      "valorEfetivo": 220.00,
      "duracaoBaseMinutos": 30,
      "duracaoPersonalizadaMinutos": 45,
      "duracaoEfetivaMinutos": 45,
      "disponivelAgendamento": true
    }
  ]
}
```

### `POST /api/me/procedimentos`

- **Projeto:** Mobile.
- **Estado/acesso:** planejada; profissional autenticado.
- **Descrição:** associa ao profissional um procedimento existente no catálogo da clínica. Não cria um procedimento global.

Entrada:

```json
{
  "procedimentoUuid": "81c3e389-738f-4d46-a992-11f78cb9a3fd",
  "valorPersonalizado": null,
  "duracaoPersonalizadaMinutos": null
}
```

Saída `201 Created`:

```json
{
  "profissionalProcedimentoUuid": "99f01d9a-a93f-4e0e-95a7-f299b1c25f12",
  "procedimentoUuid": "81c3e389-738f-4d46-a992-11f78cb9a3fd",
  "nome": "Peeling químico",
  "valorEfetivo": 300.00,
  "duracaoEfetivaMinutos": 60,
  "disponivelAgendamento": true
}
```

Associação já disponível retorna `409`. Se ela existir como indisponível, a rota reativa o mesmo registro.

### `PATCH /api/me/procedimentos/{profissionalProcedimentoUuid}`

- **Projeto:** Mobile.
- **Estado/acesso:** planejada; profissional dono da associação.
- **Descrição:** altera preço, duração ou disponibilidade do procedimento para aquele profissional, sem modificar o catálogo da clínica.

Entrada:

```json
{
  "valorPersonalizado": 240.00,
  "duracaoPersonalizadaMinutos": 50,
  "disponivelAgendamento": true
}
```

Saída `200 OK`:

```json
{
  "profissionalProcedimentoUuid": "9528014c-5161-4e5e-a8a1-ebf1c26f7417",
  "valorBase": 180.00,
  "valorPersonalizado": 240.00,
  "valorEfetivo": 240.00,
  "duracaoBaseMinutos": 30,
  "duracaoPersonalizadaMinutos": 50,
  "duracaoEfetivaMinutos": 50,
  "disponivelAgendamento": true,
  "atualizadoEm": "2026-09-02T14:30:00-03:00"
}
```

Enviar `null` em um valor personalizado restaura o padrão do catálogo. `disponivelAgendamento: false` deixa de oferecer o procedimento sem apagar o histórico.

---

## 5. Receitas do profissional

### `GET /api/me/receitas/resumo`

- **Projeto:** Mobile.
- **Estado/acesso:** planejada; profissional autenticado.
- **Descrição:** calcula o resumo financeiro do profissional a partir de `agendamentos.valor_total`, sem fornecer uma lista detalhada de atendimentos.

Entrada:

```http
GET /api/me/receitas/resumo?inicio=2026-09-01&fim=2026-10-01
Authorization: Bearer <access-token>
```

Saída `200 OK`:

```json
{
  "periodo": {
    "inicio": "2026-09-01",
    "fim": "2026-10-01"
  },
  "valorAtendimentosConcluidos": 9840.00,
  "receitaPrevista": 2640.00,
  "quantidadeAtendimentosConcluidos": 37,
  "quantidadeAgendamentosFuturos": 10,
  "ticketMedio": 265.95,
  "porProcedimento": [
    {
      "procedimentoUuid": "19ad57ea-7523-4659-8eab-6d1475831a13",
      "nome": "Consulta inicial",
      "quantidade": 22,
      "valor": 4840.00
    }
  ]
}
```

`valorAtendimentosConcluidos` soma atendimentos com status `concluido`; `receitaPrevista` soma agendamentos futuros ativos. Registros cancelados, faltas e soft deleted são desconsiderados.

---

## 6. Cobertura das telas e fluxos

### Web

| Tela ou fluxo | Rotas principais |
|---|---|
| Login e sessão | `POST /api/auth/login`, `GET /api/me` |
| Agenda da clínica | `GET /api/profissionais`, `GET /api/agenda/{profissionalUuid}` |
| Detalhes e status do atendimento | `GET /api/agendamentos/{agendamentoUuid}`, `PATCH /api/agendamentos/{agendamentoUuid}/status` |
| Novo agendamento | `POST /api/clientes/resolver`, `GET /api/profissionais`, `GET /api/profissionais/{profissionalUuid}/procedimentos`, `POST /api/agendamentos` |
| Ficha do cliente | `GET /api/clientes/{clienteUuid}` |

No Web, atendentes e profissionais iniciam o novo agendamento selecionando diretamente um intervalo livre visível no calendário. A interface não chama a rota de disponibilidade. Como a agenda exibida é apenas uma fotografia, `POST /api/agendamentos` deve revalidar o intervalo no backend antes de confirmar a gravação, protegendo o fluxo contra atualizações concorrentes.

### Mobile — Agend.AI Profissional

| Tela ou fluxo | Rotas principais |
|---|---|
| Login e perfil | `POST /api/auth/login`, `GET /api/me` |
| Agenda e atendimento | `GET /api/agenda/{profissionalUuid}`, `GET /api/agendamentos/{agendamentoUuid}` |
| Bloquear ou liberar horário | `POST /api/me/indisponibilidades`, `DELETE /api/me/indisponibilidades/{eventoId}` |
| Procedimentos e preços | `GET /api/procedimentos`, `GET /api/me/procedimentos`, `POST /api/me/procedimentos`, `PATCH /api/me/procedimentos/{profissionalProcedimentoUuid}` |
| Receitas | `GET /api/me/receitas/resumo` |

### Chatbot

| Etapa                    | Rotas principais                                            |
| ------------------------ | ----------------------------------------------------------- |
| Identificar cliente      | `POST /api/clientes/resolver`                                |
| Selecionar especialidade | `GET /api/especialidades`                                   |
| Selecionar profissional  | `GET /api/profissionais`                                    |
| Selecionar procedimento  | `GET /api/profissionais/{profissionalUuid}/procedimentos`   |
| Consultar horários       | `GET /api/profissionais/{profissionalUuid}/disponibilidade` |
| Confirmar agendamento    | `POST /api/agendamentos`                                    |

---

## 7. Dependências antes da implementação

1. **Autenticação:** adicionar JWT Bearer à API, validar assinatura, emissor, público e expiração, e armazenar a chave somente em variável de ambiente.
2. **Senha:** gerar e validar hashes seguros; nunca armazenar ou registrar a senha original.
3. **Identificação da clínica no login:** o e-mail é único apenas dentro de uma clínica. O `clinicaUuid` deve vir de configuração, convite ou informação conhecida pela interface.
4. **Dados iniciais:** clínica, usuários, horários de funcionamento, especialidades e catálogo de procedimentos precisam existir previamente no ambiente acadêmico.
5. **Clientes e Telegram:** `clientes.id_telegram` aceita `NULL` no schema, na entidade e no mapping. A rota de resolução mantém o campo opcional e índices parciais únicos impedem que telefone ou Telegram ativos sejam compartilhados por clientes da mesma clínica.
6. **Procedimentos do profissional:** `profissional_procedimentos` está sincronizada entre `database/init.sql`, entidade e mapping NHibernate, com unicidade ativa do par profissional/procedimento. Ainda falta expor a associação em uma rota de listagem para os consumidores.
7. **Schema atual:** a relação de `profissionais` com `usuarios` e a definição de `profissional_procedimentos` já estão corrigidas no bootstrap canônico. Mudanças estruturais futuras devem continuar sincronizadas com entidades e mappings.
8. **Eventos do Calendar:** padronizar o marcador `tipo = agendamento | indisponibilidade`. A consulta deve retornar `agendamentoUuid` apenas quando o evento possuir registro relacional.
9. **Bloqueios:** criar no n8n as operações de criar e excluir um evento `Indisponível`. A exclusão deve validar agenda, profissional e marcador do evento.
10. **Idempotência da criação:** implementada por chave aleatória persistida e escopada pela clínica; retentativas idênticas concluídas devolvem o mesmo agendamento.
11. **Receitas:** os valores representam atendimentos concluídos e receita prevista, não contabilidade, conciliação ou fluxo de caixa.

## Resumo quantitativo

- **22 contratos HTTP**;
- **5 rotas existentes que precisam receber autenticação e ajustes de escopo**;
- **17 rotas planejadas**;
- nenhuma listagem com busca ou paginação.

As rotas estão agrupadas por domínio para que controllers, handlers, validações e modelos possam ser reutilizados sem misturar responsabilidades.
