namespace AgendAi.Application.Profissionais.ListarMeusProcedimentos;

public sealed record MeuProcedimentoResult(
    Guid ProfissionalProcedimentoUuid,
    Guid ProcedimentoUuid,
    string Nome,
    decimal ValorEfetivo,
    int DuracaoEfetivaMinutos
);