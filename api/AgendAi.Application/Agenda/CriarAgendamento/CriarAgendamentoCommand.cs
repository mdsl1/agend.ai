namespace AgendAi.Application.Agenda.CriarAgendamento;

public sealed record CriarAgendamentoCommand (
    Guid ClienteUuid,
    Guid ProfissionalProcedimentoUuid,
    DateTimeOffset Inicio,
    string? MotivoContato,
    string IdempotencyKey
);