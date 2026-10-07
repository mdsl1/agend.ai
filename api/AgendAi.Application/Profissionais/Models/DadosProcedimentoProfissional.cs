namespace AgendAi.Application.Profissionais.Models;

public sealed record DadosProcedimentoProfissional(
    Guid ProfissionalProcedimentoUuid,
    Guid ProcedimentoUuid,
    string Nome,
    decimal Valor,
    int DuracaoMinutos
);