namespace AgendAi.API.Contracts.Overview;

public sealed record OverviewReceitasResponse(
    OverviewReceitasResponse.PeriodoResponse Periodo,
    decimal ValorTotal,
    decimal? PercentualVsMesAnterior
)
{
    public sealed record PeriodoResponse(
        DateOnly Inicio,
        DateOnly Fim
    );
}