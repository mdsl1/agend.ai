namespace AgendAi.Application.Overview.ConsultarMeuOverview;

public sealed record OverviewReceitasResult(
    OverviewReceitasResult.PeriodoResult Periodo,
    decimal ValorTotal,
    decimal? PercentualVsMesAnterior
)
{
    public sealed record PeriodoResult(
        DateOnly Inicio,
        DateOnly Fim
    );
}