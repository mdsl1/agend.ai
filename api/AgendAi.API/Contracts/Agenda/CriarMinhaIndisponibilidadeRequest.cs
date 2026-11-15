namespace AgendAi.API.Contracts.Agenda;

public sealed record CriarMinhaIndisponibilidadeRequest(
    DateTimeOffset Inicio,
    DateTimeOffset Fim,
    string? Motivo,
    bool SeRepete,
    string? Frequencia,
    DateTimeOffset? RepetirAte
);