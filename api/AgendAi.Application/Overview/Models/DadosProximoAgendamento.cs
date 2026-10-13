namespace AgendAi.Application.Overview.Models;

public sealed record DadosProximoAgendamento(
    Guid AgendamentoUuid,
    DateTimeOffset Inicio,
    DateTimeOffset Fim,
    string NomeCliente,
    string NomeProcedimento,
    string Status
);