namespace AgendAi.API.Contracts.Agendamentos;

public sealed record AlterarStatusAgendamentoResponse(
    Guid AgendamentoUuid,
    string StatusAnterior,
    string NovoStatus,
    DateTimeOffset AlteradoEm
);