namespace AgendAi.Application.Agenda.ConsultarAgenda;

public sealed record EventoAgendaResult (
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
