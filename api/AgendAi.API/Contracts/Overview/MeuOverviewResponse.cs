namespace AgendAi.API.Contracts.Overview;
public sealed record MeuOverviewResponse(
    DateOnly DataReferencia,
    OverviewReceitasResponse ResumoReceitas,
    OverviewAgendaResponse ResumoAgenda
);