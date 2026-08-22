namespace AgendAi.Application.Agenda.ListarAgendas;

public sealed record ListarAgendasResult (
    IReadOnlyCollection<AgendaDisponivelResult> Agendas
);