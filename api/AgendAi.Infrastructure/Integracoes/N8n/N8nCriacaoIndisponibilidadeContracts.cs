namespace AgendAi.Infrastructure.Integracoes.N8n;

using System.Text.Json.Serialization;

internal sealed record CriarIndisponibilidadeN8nRequest(
    [property: JsonPropertyName("operacao")]
    string Operacao,

    [property: JsonPropertyName("evento")]
    EventoCriacaoIndisponibilidadeN8nRequest Evento,

    [property: JsonPropertyName("profissional")]
    ProfissionalCriacaoIndisponibilidadeN8nRequest Profissional,

    [property: JsonPropertyName("motivo")]
    string Motivo
);

internal sealed record EventoCriacaoIndisponibilidadeN8nRequest(
    [property: JsonPropertyName("hora_inicio")]
    DateTimeOffset HoraInicio,

    [property: JsonPropertyName("hora_fim")]
    DateTimeOffset HoraFim,

    [property: JsonPropertyName("se_repete")]
    bool SeRepete,

    [property: JsonPropertyName("frequencia")]
    string? Frequencia,

    [property: JsonPropertyName("repetir_ate")]
    DateTimeOffset? RepetirAte
);

internal sealed record ProfissionalCriacaoIndisponibilidadeN8nRequest(
    [property: JsonPropertyName("id_calendar")]
    string IdCalendar
);

internal sealed record CriarIndisponibilidadeN8nResponse(
    [property: JsonPropertyName("sucesso")]
    bool Sucesso,

    [property: JsonPropertyName("id_event")]
    string? IdEvent,

    [property: JsonPropertyName("horario_solicitado")]
    HorarioSolicitadoIndisponibilidadeN8nResponse? HorarioSolicitado,

    [property: JsonPropertyName("frequencia")]
    string? Frequencia,

    [property: JsonPropertyName("repete_ate")]
    DateTimeOffset? RepeteAte
);

internal sealed record HorarioSolicitadoIndisponibilidadeN8nResponse(
    [property: JsonPropertyName("inicio")]
    DateTimeOffset Inicio,

    [property: JsonPropertyName("fim")]
    DateTimeOffset Fim
);