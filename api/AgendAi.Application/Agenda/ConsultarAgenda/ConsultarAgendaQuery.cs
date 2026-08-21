namespace AgendAi.Application.Agenda.ConsultarAgenda;

public sealed record ConsultarAgendaQuery (
    Guid ProfissionalUuid,
    DateTimeOffset Inicio,
    DateTimeOffset Fim
);