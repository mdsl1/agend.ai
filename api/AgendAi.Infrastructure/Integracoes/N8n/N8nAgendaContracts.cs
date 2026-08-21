namespace AgendAi.Infrastructure.Integracoes.N8n;
using System.Text.Json.Serialization;

internal sealed record ConsultarAgendaN8nRequest(
    [property: JsonPropertyName("operacao")]
    string Operacao,

    [property: JsonPropertyName("profissional")]
    ProfissionalN8nRequest Profissional,

    [property: JsonPropertyName("periodo")]
    PeriodoN8nRequest Periodo
);
internal sealed record ProfissionalN8nRequest(
    [property: JsonPropertyName("uuid")]
    Guid Uuid,

    [property: JsonPropertyName("id_calendar")]
    string IdCalendar
);
internal sealed record PeriodoN8nRequest(
    [property: JsonPropertyName("inicio")]
    DateTimeOffset Inicio,

    [property: JsonPropertyName("fim")]
    DateTimeOffset Fim
);
internal sealed record ConsultarAgendaN8nResponse(
    [property: JsonPropertyName("sucesso")]
    bool Sucesso,
    
    [property: JsonPropertyName("agendamentos")]
    IReadOnlyCollection<AgendamentoN8nResponse>? Agendamentos
);
internal sealed record AgendamentoN8nResponse(
    [property: JsonPropertyName("id")]
    string Id,

    [property: JsonPropertyName("title")]
    string Titulo,

    [property: JsonPropertyName("inicio")]
    DateTimeOffset Inicio,

    [property: JsonPropertyName("fim")]
    DateTimeOffset Fim,
    
    
    [property: JsonPropertyName("extendedProps")]
    ExtendedPropsN8nResponse? ExtendedProps
);

internal sealed record ExtendedPropsN8nResponse(
    [property: JsonPropertyName("patient")]
    string? Cliente,
    
    [property: JsonPropertyName("procedure")]
    string? Procedimento,
    
    [property: JsonPropertyName("doctorUuid")]
    Guid? ProfissionalUuid
);
