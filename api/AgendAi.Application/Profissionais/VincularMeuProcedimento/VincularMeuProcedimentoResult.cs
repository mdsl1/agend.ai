namespace AgendAi.Application.Profissionais.VincularMeuProcedimento;

public sealed record VincularMeuProcedimentoResult(
    Guid ProfissionalProcedimentoUuid,
    bool Criado,
    string Nome,
    int DuracaoMinutos,
    decimal Valor
);