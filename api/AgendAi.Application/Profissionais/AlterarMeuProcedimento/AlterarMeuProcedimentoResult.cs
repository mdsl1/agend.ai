namespace AgendAi.Application.Profissionais.AlterarMeuProcedimento;

public sealed record AlterarMeuProcedimentoResult(
    Guid ProfissionalProcedimentoUuid,
    bool DisponivelAgendamento,
    string Nome,
    int DuracaoMinutos,
    decimal Valor
);