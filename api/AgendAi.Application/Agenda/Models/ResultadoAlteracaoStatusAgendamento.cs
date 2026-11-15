namespace AgendAi.Application.Agenda.Models;

public sealed record ResultadoAlteracaoStatusAgendamento(
    Guid AgendamentoUuid,
    string StatusAnterior,
    string StatusPgtoAnterior,
    string NovoStatus,
    string NovoStatusPgto,
    DateTimeOffset AlteradoEm,
    string? IdEventoExterno,
    string? IdAgendaExterna,
    string? UrlWebhookAgenda
);