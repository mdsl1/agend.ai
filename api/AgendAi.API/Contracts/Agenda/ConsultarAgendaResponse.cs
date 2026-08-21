namespace AgendAi.API.Contracts.Agenda;

public sealed record ConsultarAgendaResponse (
    IReadOnlyCollection<EventoAgendaResponse> Eventos
);