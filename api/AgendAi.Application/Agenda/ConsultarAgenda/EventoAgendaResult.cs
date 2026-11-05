namespace AgendAi.Application.Agenda.ConsultarAgenda;

public sealed record EventoAgendaResult (
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
