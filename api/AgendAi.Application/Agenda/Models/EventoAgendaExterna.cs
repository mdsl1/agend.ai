namespace AgendAi.Application.Agenda.Models;

public sealed record EventoAgendaExterna (
    string IdExterno,
    string Titulo,
    string Tipo,
    DateTimeOffset Inicio,
    DateTimeOffset Fim,
    string? NomeCliente,
    string? NomeProcedimento,
    Guid ProfissionalUuid
);
