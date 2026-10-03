namespace AgendAi.API.Contracts.Agendamentos;

public sealed record ConsultarAgendamentoResponse(
    Guid Uuid,
    ClienteAgendamentoResponse Cliente,
    ProfissionalAgendamentoResponse Profissional,
    ProcedimentoAgendamentoResponse Procedimento,
    DateTimeOffset Inicio,
    DateTimeOffset Fim,
    string? MotivoContato,
    string? AnotacoesProfissional,
    decimal ValorTotal,
    string Status,
    string StatusPagamento
);