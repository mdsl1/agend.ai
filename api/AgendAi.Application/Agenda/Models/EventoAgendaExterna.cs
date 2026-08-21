namespace AgendAi.Application.Agenda.Models;

public sealed record EventoAgendaExterna (
    string IdExterno,
    string Titulo,
    DateTimeOffset Inicio,
    DateTimeOffset Fim,
    string NomeCliente,
    string? NomeProcedimento,
    Guid ProfissionalUuid
);