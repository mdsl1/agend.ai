namespace AgendAi.Application.Agenda.AlterarStatusAgendamento;

public sealed record AlterarStatusAgendamentoResult(
    Guid AgendamentoUuid,
    string StatusAnterior,
    string NovoStatus,
    DateTimeOffset AlteradoEm
);