namespace AgendAi.API.Contracts.Profissionais;

public sealed record ProcedimentoProfissionalResponse(
    Guid ProfissionalProcedimentoUuid,
    Guid ProcedimentoUuid,
    string Nome,
    decimal ValorEfetivo,
    int DuracaoEfetivaMinutos
);
