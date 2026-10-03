namespace AgendAi.API.Contracts.Agendamentos;

public sealed record ProfissionalAgendamentoResponse(
    Guid Uuid,
    string NomeExibicao
);