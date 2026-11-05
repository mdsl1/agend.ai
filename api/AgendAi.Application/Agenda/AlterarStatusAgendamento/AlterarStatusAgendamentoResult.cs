namespace AgendAi.Application.Agenda.AlterarStatusAgendamento;

public sealed record AlterarStatusAgendamentoResult(
    Guid AgendamentoUuid,
    string StatusAnterior,
    string StatusPgtoAnterior,
    string NovoStatus,
    string NovoStatusPgto,
    DateTimeOffset AlteradoEm
);