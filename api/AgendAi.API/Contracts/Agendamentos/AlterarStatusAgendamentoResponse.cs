namespace AgendAi.API.Contracts.Agendamentos;

public sealed record AlterarStatusAgendamentoResponse(
    Guid AgendamentoUuid,
    string StatusAnterior,
    string StatusPagamentoAnterior,
    string NovoStatus,
    string NovoStatusPagamento,
    DateTimeOffset AlteradoEm
);