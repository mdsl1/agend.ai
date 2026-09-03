namespace AgendAi.API.Contracts.Agenda;

public sealed record EventoAgendaResponse (
    string Id,
    string Titulo,
    string Tipo,
    Guid? AgendamentoUuid,
    DateTimeOffset Inicio,
    DateTimeOffset Fim,
    string? NomeCliente,
    string? NomeProcedimento,
    Guid ProfissionalUuid
);
