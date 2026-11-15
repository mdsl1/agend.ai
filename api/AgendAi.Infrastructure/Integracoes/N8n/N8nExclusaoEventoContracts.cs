namespace AgendAi.Infrastructure.Integracoes.N8n;

using System.Text.Json.Serialization;

internal sealed record ExcluirEventoN8nRequest(
    [property: JsonPropertyName("operacao")]
    string Operacao,

    [property: JsonPropertyName("evento")]
    EventoExclusaoN8nRequest Evento,

    [property: JsonPropertyName("profissional")]
    ProfissionalExclusaoEventoN8nRequest Profissional
);

internal sealed record EventoExclusaoN8nRequest(
    [property: JsonPropertyName("id_evento_calendar")]
    string IdEventoCalendar
);

internal sealed record ProfissionalExclusaoEventoN8nRequest(
    [property: JsonPropertyName("id_calendar")]
    string IdCalendar
);

internal sealed record ExcluirEventoN8nResponse(
    [property: JsonPropertyName("sucesso")]
    bool Sucesso,

    [property: JsonPropertyName("id_event")]
    string? IdEvent
);