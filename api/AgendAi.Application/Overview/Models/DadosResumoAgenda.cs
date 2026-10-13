namespace AgendAi.Application.Overview.Models;

public sealed record DadosResumoAgenda(
    int QtdeHoje,
    int QtdeConfirmados,
    int QtdeRestante,
    int QtdeCancelados,
    DadosProximoAgendamento? ProximoAgendamento
);