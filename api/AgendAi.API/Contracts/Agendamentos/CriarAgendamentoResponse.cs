namespace AgendAi.API.Contracts.Agendamentos;

public sealed record CriarAgendamentoResponse(
    Guid Uuid,
    DateTimeOffset Inicio,
    DateTimeOffset Fim,
    string NomeCliente,
    string NomeProfissional,
    string NomeProcedimento,
    decimal ValorTotal,
    string Status
);