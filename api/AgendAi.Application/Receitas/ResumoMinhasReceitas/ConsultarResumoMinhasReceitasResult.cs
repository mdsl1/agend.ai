namespace AgendAi.Application.Receitas.ResumoMinhasReceitas;

public sealed record ConsultarResumoMinhasReceitasResult(
    ConsultarResumoMinhasReceitasResult.PeriodoResult Periodo,
    decimal ReceitaRecebida,
    decimal ReceitaEstimada,
    decimal? PercentualVsMesAnterior,
    IReadOnlyCollection<MinhasReceitasPorProcedimentoResult> PorProcedimento
)
{
    public sealed record PeriodoResult(
        DateOnly Inicio,
        DateOnly Fim
    );
}