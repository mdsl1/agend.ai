# Rotas da API RESTful — MVP Acadêmico Web, Mobile e Chatbot

> Última revisão: 2 de setembro de 2026.

Este documento define os contratos HTTP necessários para integrar os três projetos:

- **Web:** CRM React utilizado pela clínica;
- **Mobile:** aplicativo **Agend.AI Profissional**, desenvolvido com React Native, Expo e TypeScript;
- **Chatbot:** workflow n8n que atende o cliente pelo Telegram.

## Estado atual

As seguintes rotas já existem na PoC:

- `GET /api/agendas?clinicaUuid={clinicaUuid}`;
- `GET /api/agenda/{profissionalUuid}?inicio={iso}&fim={iso}`.

Elas ainda não possuem autenticação. Os demais contratos estão planejados e todos devem respeitar o escopo da clínica e o RBAC.

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
- Consultas de agenda e disponibilidade aceitam períodos de no máximo 45 dias.

### O que é idempotência?

Idempotência significa que repetir a mesma operação produz o mesmo resultado, sem duplicar seus efeitos. Ela é especialmente importante quando uma requisição pode ser reenviada após timeout, falha de rede ou retentativa do n8n.

Em uma criação de agendamento, o consumidor envia uma chave exclusiva para aquela ação:

```http
POST /api/agendamentos
Idempotency-Key: telegram:982360:criar-agendamento
```

Na primeira chamada, a API cria o recurso e associa a resposta à chave. Se a mesma requisição for repetida com a mesma chave, a API devolve o resultado original em vez de criar outro agendamento. A mesma chave com um corpo diferente deve retornar `409 Conflict`.

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

### `GET /api/agendas`

- **Projeto:** Web.
- **Estado/acesso:** implementada na PoC sem autenticação; deverá ser protegida.
- **Descrição:** lista as agendas configuradas para preencher o seletor de profissionais do CRM.

Entrada atual da PoC:

```http
GET /api/agendas?clinicaUuid=20d84992-1f87-42cc-a251-92cac362f41f
```

Entrada desejada:

```http
GET /api/agendas
Authorization: Bearer <access-token>
```

Saída `200 OK`:

```json
{
  "agendas": [
    {
      "profissionalUuid": "abf43724-5590-4475-a11f-b679e285477d",
      "nomeExibicao": "Dra. Ana Lima"
    }
  ]
}
```

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
      "clienteNome": "João Silva",
      "procedimentoNome": "Consulta inicial",
      "profissionalUuid": "abf43724-5590-4475-a11f-b679e285477d"
    },
    {
      "id": "evento-opaco-456",
      "tipo": "indisponibilidade",
      "agendamentoUuid": null,
      "titulo": "Indisponível",
      "inicio": "2026-09-02T12:00:00-03:00",
      "fim": "2026-09-02T13:00:00-03:00",
      "clienteNome": null,
      "procedimentoNome": null,
      "profissionalUuid": "abf43724-5590-4475-a11f-b679e285477d"
    }
  ]
}
```

O campo `tipo` define o componente visual usado pelo Web e pelo Mobile. Um profissional só pode consultar seu próprio UUID.

### `GET /api/profissionais/{profissionalUuid}/disponibilidade`

- **Projeto:** Web e Chatbot.
- **Estado/acesso:** planejada; usuário autorizado ou identidade de serviço.
- **Descrição:** calcula horários livres considerando horário de funcionamento, duração do procedimento, agendamentos e eventos de indisponibilidade. O Mobile não usa esta rota.

Entrada:

```http
GET /api/profissionais/abf43724-5590-4475-a11f-b679e285477d/disponibilidade?profissionalProcedimentoUuid=9528014c-5161-4e5e-a8a1-ebf1c26f7417&inicio=2026-09-03T00:00:00-03:00&fim=2026-09-10T00:00:00-03:00&limite=3
```

Saída `200 OK`:

```json
{
  "duracaoMinutos": 45,
  "horarios": [
    {
      "inicio": "2026-09-03T09:00:00-03:00",
      "fim": "2026-09-03T09:45:00-03:00"
    },
    {
      "inicio": "2026-09-03T10:00:00-03:00",
      "fim": "2026-09-03T10:45:00-03:00"
    }
  ]
}
```

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
- **Estado/acesso:** planejada; usuário autorizado ou identidade de serviço.
- **Descrição:** cria o evento no Google Calendar e registra o agendamento no PostgreSQL. A API deriva profissional, procedimento, duração, fim e valor da associação `profissional_procedimentos`.

Entrada:

```http
POST /api/agendamentos
Idempotency-Key: telegram:982360:criar-agendamento
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
  "inicio": "2026-09-10T15:00:00-03:00",
  "fim": "2026-09-10T15:45:00-03:00",
  "profissionalNome": "Dra. Ana Lima",
  "procedimentoNome": "Consulta inicial",
  "valorTotal": 220.00,
  "status": "agendado"
}
```

Se o intervalo estiver ocupado, retorna `409 horario_indisponivel`. A constraint de sobreposição do banco continua sendo a barreira final contra concorrência.

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

### `POST /api/integracoes/chatbot/clientes/resolver`

- **Projeto:** Chatbot.
- **Estado/acesso:** planejada; identidade de serviço do n8n.
- **Descrição:** procura atomicamente um cliente pelo telefone normalizado dentro da clínica. Se não existir, cria; se existir, atualiza o vínculo com o identificador do Telegram quando permitido.

Entrada:

```http
POST /api/integracoes/chatbot/clientes/resolver
Authorization: Bearer <token-de-servico>
X-AgendAi-Clinic-Uuid: 20d84992-1f87-42cc-a251-92cac362f41f
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
  "telefone": "+5511999991234",
  "urlPerfil": "https://app.agend.ai/clientes/438d71b4-197c-44d9-bf1e-e6e84e983e67"
}
```

O telefone normalizado é a identidade de deduplicação do cliente dentro da clínica.

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

- **Projeto:** Chatbot.
- **Estado/acesso:** planejada; identidade de serviço do n8n.
- **Descrição:** lista os profissionais ativos de uma especialidade que possuem uma agenda configurada.

Entrada:

```http
GET /api/profissionais?especialidadeUuid=443445f5-3922-4238-b83e-ef8b083110d2
Authorization: Bearer <token-de-servico>
X-AgendAi-Clinic-Uuid: 20d84992-1f87-42cc-a251-92cac362f41f
```

Saída `200 OK`:

```json
{
  "profissionais": [
    {
      "uuid": "abf43724-5590-4475-a11f-b679e285477d",
      "nomeExibicao": "Dra. Ana Lima",
      "especialidade": {
        "uuid": "443445f5-3922-4238-b83e-ef8b083110d2",
        "nome": "Dermatologia"
      }
    }
  ]
}
```

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
| Agenda da clínica | `GET /api/agendas`, `GET /api/agenda/{profissionalUuid}` |
| Detalhes e status do atendimento | `GET /api/agendamentos/{agendamentoUuid}`, `PATCH /api/agendamentos/{agendamentoUuid}/status` |
| Novo agendamento | `GET /api/clientes`, `POST /api/clientes`, `GET /api/profissionais/{profissionalUuid}/procedimentos`, `GET /api/profissionais/{profissionalUuid}/disponibilidade`, `POST /api/agendamentos` |
| Ficha do cliente | `GET /api/clientes/{clienteUuid}` |

### Mobile — Agend.AI Profissional

| Tela ou fluxo | Rotas principais |
|---|---|
| Login e perfil | `POST /api/auth/login`, `GET /api/me` |
| Agenda e atendimento | `GET /api/agenda/{profissionalUuid}`, `GET /api/agendamentos/{agendamentoUuid}` |
| Bloquear ou liberar horário | `POST /api/me/indisponibilidades`, `DELETE /api/me/indisponibilidades/{eventoId}` |
| Procedimentos e preços | `GET /api/procedimentos`, `GET /api/me/procedimentos`, `POST /api/me/procedimentos`, `PATCH /api/me/procedimentos/{profissionalProcedimentoUuid}` |
| Receitas | `GET /api/me/receitas/resumo` |

### Chatbot

| Etapa | Rotas principais |
|---|---|
| Identificar cliente | `POST /api/integracoes/chatbot/clientes/resolver` |
| Selecionar especialidade | `GET /api/especialidades` |
| Selecionar profissional | `GET /api/profissionais` |
| Selecionar procedimento | `GET /api/profissionais/{profissionalUuid}/procedimentos` |
| Consultar horários | `GET /api/profissionais/{profissionalUuid}/disponibilidade` |
| Confirmar agendamento | `POST /api/agendamentos` |

---

## 7. Dependências antes da implementação

1. **Autenticação:** adicionar JWT Bearer à API, validar assinatura, emissor, público e expiração, e armazenar a chave somente em variável de ambiente.
2. **Senha:** gerar e validar hashes seguros; nunca armazenar ou registrar a senha original.
3. **Identificação da clínica no login:** o e-mail é único apenas dentro de uma clínica. O `clinicaUuid` deve vir de configuração, convite ou informação conhecida pela interface.
4. **Dados iniciais:** clínica, usuários, horários de funcionamento, especialidades e catálogo de procedimentos precisam existir previamente no ambiente acadêmico.
5. **Clientes e Telegram:** `clientes.id_telegram` deve aceitar `NULL`, pois um cadastro manual pelo Web pode não possuir vínculo com o Telegram. Entidade e mapping também precisam representar o campo.
6. **Procedimentos do profissional:** sincronizar `profissional_procedimentos` entre `database/init.sql`, entidade e mapping NHibernate. A associação deve impedir duplicidade do mesmo procedimento para o mesmo profissional.
7. **Schema atual:** corrigir a autorreferência indevida da tabela `profissionais` no lugar de `id_usuario` e o texto `NUT NULL` presente em `profissional_procedimentos` antes de recriar o banco.
8. **Eventos do Calendar:** padronizar o marcador `tipo = agendamento | indisponibilidade`. A consulta deve retornar `agendamentoUuid` apenas quando o evento possuir registro relacional.
9. **Bloqueios:** criar no n8n as operações de criar e excluir um evento `Indisponível`. A exclusão deve validar agenda, profissional e marcador do evento.
10. **Idempotência:** persistir a chave da criação de agendamento, ou uma identidade externa equivalente, para que retentativas devolvam o mesmo resultado.
11. **Receitas:** os valores representam atendimentos concluídos e receita prevista, não contabilidade, conciliação ou fluxo de caixa.

## Resumo quantitativo

- **22 contratos HTTP**;
- **2 rotas existentes que precisam receber autenticação e ajustes de contrato**;
- **20 rotas planejadas**;
- nenhuma listagem com busca ou paginação.

As rotas estão agrupadas por domínio para que controllers, handlers, validações e modelos possam ser reutilizados sem misturar responsabilidades.
