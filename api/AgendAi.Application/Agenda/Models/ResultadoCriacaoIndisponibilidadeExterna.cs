namespace AgendAi.Application.Agenda.Models;

public sealed record ResultadoCriacaoIndisponibilidadeExterna(
    string IdEventoExterno,
    DateTimeOffset Inicio,
    DateTimeOffset Fim,
    string? Frequencia,
    DateTimeOffset? RepetirAte
);