namespace AgendAi.API.Contracts.Agendamentos;

public sealed record ClienteAgendamentoResponse(
    Guid Uuid,
    string Nome,
    string Telefone
);