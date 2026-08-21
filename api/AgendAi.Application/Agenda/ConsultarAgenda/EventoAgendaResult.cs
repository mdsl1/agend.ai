namespace AgendAi.Application.Agenda.ConsultarAgenda;

public sealed record EventoAgendaResult (
    string Id,
    string Titulo,
    DateTimeOffset Inicio,
    DateTimeOffset Fim,
    string NomeCliente,
    string? NomeProcedimento,
    Guid ProfissionalUuid
);