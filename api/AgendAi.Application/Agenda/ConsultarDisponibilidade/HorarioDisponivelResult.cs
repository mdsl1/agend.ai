namespace AgendAi.Application.Agenda.ConsultarDisponibilidade;

public sealed record HorarioDisponivelResult (
    DateTimeOffset Inicio,
    DateTimeOffset Fim
);