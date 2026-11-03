namespace AgendAi.Application.Agenda.Models;

public sealed record ResultadoAlteracaoStatusAgendamento(
    Guid AgendamentoUuid,
    string StatusAnterior,
    string NovoStatus,
    DateTimeOffset AlteradoEm
);