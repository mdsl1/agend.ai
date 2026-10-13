namespace AgendAi.Application.Overview.ConsultarMeuOverview;

public sealed record OverviewAgendaResult(
    int QtdeHoje,
    int QtdeConfirmados,
    int QtdeRestante,
    int QtdeCancelados,
    OverviewAgendaResult.ProximoAgendamentoResult? ProximoAgendamento
)
{
    public sealed record ProximoAgendamentoResult(
        Guid AgendamentoUuid,
        DateTimeOffset Inicio,
        DateTimeOffset Fim,
        string NomeCliente,
        string NomeProcedimento,
        string Status
    );
}