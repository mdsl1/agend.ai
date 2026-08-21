namespace AgendAi.API.Contracts.Agenda;

public sealed record EventoAgendaResponse (
    string Id,
    string Titulo,
    DateTimeOffset Inicio,
    DateTimeOffset Fim,
    string NomeCliente,
    string? NomeProcedimento,
    Guid ProfissionalUuid
);