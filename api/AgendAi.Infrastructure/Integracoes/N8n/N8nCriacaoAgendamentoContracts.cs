namespace AgendAi.Infrastructure.Integracoes.N8n;

using System.Text.Json.Serialization;

internal sealed record CriarAgendamentoN8nRequest(
    [property: JsonPropertyName("operacao")]
    string Operacao,

    [property: JsonPropertyName("evento")]
    EventoCriacaoN8nRequest Evento,

    [property: JsonPropertyName("profissional")]
    ProfissionalCriacaoAgendamentoN8nRequest Profissional,

    [property: JsonPropertyName("cliente")]
    ClienteCricaoAgendamentoN8nRequest Cliente
);

internal sealed record EventoCriacaoN8nRequest(
    [property: JsonPropertyName("uuid")]
    Guid Uuid,

    [property: JsonPropertyName("id_evento_calendar")]
    string IdEventoCalendar,

    [property: JsonPropertyName("hora_inicio")]
    DateTimeOffset HoraInicio,

    [property: JsonPropertyName("hora_fim")]
    DateTimeOffset HoraFim,

    [property: JsonPropertyName("motivo_contato")]
    string? MotivoContato
);

internal sealed record ProfissionalCriacaoAgendamentoN8nRequest(
    [property: JsonPropertyName("uuid")]
    Guid Uuid,

    [property: JsonPropertyName("id_calendar")]
    string IdCalendar,

    [property: JsonPropertyName("procedimento")]
    string Procedimento
);

internal sealed record ClienteCricaoAgendamentoN8nRequest(
    [property: JsonPropertyName("nome")]
    string Nome
);

internal sealed record CriarAgendamentoN8nResponse(
    [property: JsonPropertyName("sucesso")]
    bool Sucesso,

    [property: JsonPropertyName("id_event")]
    string IdEvent,

    [property: JsonPropertyName("uuid_agendamento")]
    Guid? AgendamentoUuid,

    [property: JsonPropertyName("horario_solicitado")]
    HorarioSolicitadoCriacaoAgendamentoN8nResponse? HorarioSolicitado
);

internal sealed record HorarioSolicitadoCriacaoAgendamentoN8nResponse(
    [property: JsonPropertyName("inicio")]
    DateTimeOffset Inicio,

    [property: JsonPropertyName("fim")]
    DateTimeOffset Fim
);