namespace AgendAi.API.Contracts.Receitas;

public sealed record ResumoMinhasReceitasResponse(
    ResumoMinhasReceitasResponse.PeriodoResponse Periodo,
    decimal ReceitaRecebida,
    decimal ReceitaEstimada,
    decimal? PercentualVsMesAnterior,
    IReadOnlyCollection<MinhasReceitasPorProcedimentoResponse> PorProcedimento
)
{
    public sealed record PeriodoResponse(
        DateOnly Inicio,
        DateOnly Fim
    );
}