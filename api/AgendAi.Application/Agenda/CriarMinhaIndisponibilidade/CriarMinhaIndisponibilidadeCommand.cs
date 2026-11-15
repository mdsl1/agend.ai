namespace AgendAi.Application.Agenda.CriarMinhaIndisponibilidade;

public sealed record CriarMinhaIndisponibilidadeCommand(
    DateTimeOffset Inicio,
    DateTimeOffset Fim,
    string? Motivo,
    bool SeRepete,
    string? Frequencia,
    DateTimeOffset? RepetirAte
);