namespace AgendAi.Application.Agenda.Models;

public sealed record CriacaoIndisponibilidadeExterna(
    string IdAgendaExterna,
    DateTimeOffset Inicio,
    DateTimeOffset Fim,
    string Motivo,
    bool SeRepete,
    string? Frequencia,
    DateTimeOffset? RepetirAte
);