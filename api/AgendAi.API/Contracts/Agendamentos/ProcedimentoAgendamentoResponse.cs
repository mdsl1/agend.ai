namespace AgendAi.API.Contracts.Agendamentos;

public sealed record ProcedimentoAgendamentoResponse(
    Guid Uuid,
    string Nome,
    int DuracaoMinutos
);