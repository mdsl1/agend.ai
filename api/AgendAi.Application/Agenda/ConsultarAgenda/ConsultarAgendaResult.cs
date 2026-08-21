namespace AgendAi.Application.Agenda.ConsultarAgenda;

public sealed record ConsultarAgendaResult (
    IReadOnlyCollection<EventoAgendaResult> Eventos
);