namespace AgendAi.API.Contracts.Agenda;

public sealed record EventoAgendaResponse (
    string Id,
    string Titulo,
    string Tipo,
    Guid? AgendamentoUuid,
    string? StatusAgendamento,
    string? StatusPagamento,
    DateTimeOffset Inicio,
    DateTimeOffset Fim,
    string? NomeCliente,
    string? NomeProcedimento,
    Guid ProfissionalUuid
);
