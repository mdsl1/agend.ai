namespace AgendAi.API.Contracts.Profissionais;

public sealed record AlterarMeuProcedimentoResponse(
    Guid ProfissionalProcedimentoUuid,
    bool DisponivelAgendamento,
    string Nome,
    int DuracaoMinutos,
    decimal Valor
);