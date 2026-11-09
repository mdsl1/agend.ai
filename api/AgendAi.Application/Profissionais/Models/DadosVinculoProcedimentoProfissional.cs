namespace AgendAi.Application.Profissionais.Models;

public sealed record DadosVinculoProcedimentoProfissional(
    Guid ProcedimentoUuid,
    string Nome,
    int DuracaoEstimadaMinutos,
    decimal ValorBase,
    Guid? ProfissionalProcedimentoUuid,
    int? DuracaoMinutos,
    decimal? Valor,
    DateTime? DeletedAt
)
{
    public bool VinculoExistente => ProfissionalProcedimentoUuid.HasValue;

    public bool VinculoAtivo => ProfissionalProcedimentoUuid.HasValue && DeletedAt is null;
}