namespace AgendAi.Application.Agenda.Models;

public sealed record DadosAgendaDisponivel (
    Guid ProfissionalUuid,
    string Nome,
    string? Prefixo
);