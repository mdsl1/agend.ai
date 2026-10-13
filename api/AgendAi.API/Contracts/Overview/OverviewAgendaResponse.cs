namespace AgendAi.API.Contracts.Overview;

public sealed record OverviewAgendaResponse(
    int QtdeHoje,
    int QtdeConfirmados,
    int QtdeRestante,
    int QtdeCancelados,
    OverviewAgendaResponse.ProximoAgendamentoResponse? ProximoAgendamento
)
{
    public sealed record ProximoAgendamentoResponse(
        Guid AgendamentoUuid,
        DateTimeOffset Inicio,
        DateTimeOffset Fim,
        string NomeCliente,
        string NomeProcedimento,
        string Status
    );
}