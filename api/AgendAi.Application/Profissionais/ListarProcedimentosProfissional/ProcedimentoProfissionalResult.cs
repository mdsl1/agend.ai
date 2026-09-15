namespace AgendAi.Application.Profissionais.ListarProcedimentosProfissional;

public sealed record ProcedimentoProfissionalResult(
    Guid ProfissionalProcedimentoUuid,
    Guid ProcedimentoUuid,
    string Nome,
    decimal ValorEfetivo,
    int DuracaoEfetivaMinutos
);