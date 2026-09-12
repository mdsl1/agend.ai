namespace AgendAi.Application.Agenda.CriarAgendamento;

public sealed record CriarAgendamentoResult(
    Guid AgendamentoUuid,
    DateTimeOffset Inicio,
    DateTimeOffset Fim,
    string NomeCliente,
    string NomeProfissional,
    string NomeProcedimento,
    decimal ValorTotal,
    string Status
);