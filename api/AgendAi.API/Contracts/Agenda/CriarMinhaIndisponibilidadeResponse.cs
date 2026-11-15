namespace AgendAi.API.Contracts.Agenda;

public sealed record CriarMinhaIndisponibilidadeResponse(
    string Id,
    string Tipo,
    string Titulo,
    DateTimeOffset Inicio,
    DateTimeOffset Fim,
    bool SeRepete,
    string? Frequencia,
    DateTimeOffset? RepetirAte
);