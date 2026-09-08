namespace AgendAi.Application.Agenda.Models;

public sealed record HorarioDisponivelExterno (
    DateTimeOffset Inicio,
    DateTimeOffset Fim
);