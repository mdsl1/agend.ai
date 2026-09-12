namespace AgendAi.Application.Agenda.Models;

public sealed record CriacaoAgendamentoExterno(
    Guid AgendamentoUuid,
    DateTimeOffset Inicio,
    DateTimeOffset Fim,
    string? MotivoContato,
    Guid ProfissionalUuid,
    string IdAgendaExterna,
    string NomeProcedimento,
    string NomeCliente
);