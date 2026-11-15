namespace AgendAi.Application.Agenda.CriarMinhaIndisponibilidade;

public sealed record CriarMinhaIndisponibilidadeResult(
    string Id,
    string Tipo,
    string Titulo,
    DateTimeOffset Inicio,
    DateTimeOffset Fim,
    bool SeRepete,
    string? Frequencia,
    DateTimeOffset? RepetirAte
);