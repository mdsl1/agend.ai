namespace AgendAi.Application.Overview.ConsultarMeuOverview;

public sealed record ConsultarMeuOverviewResult(
    DateOnly DataReferencia,
    OverviewAgendaResult ResumoAgenda,
    OverviewReceitasResult ResumoReceitas
);