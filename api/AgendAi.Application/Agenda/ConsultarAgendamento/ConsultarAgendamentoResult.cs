namespace AgendAi.Application.Agenda.ConsultarAgendamento;

public sealed record ConsultarAgendamentoResult(
    Guid Uuid,
    ClienteAgendamentoResult Cliente,
    ProfissionalAgendamentoResult Profissional,
    ProcedimentoAgendamentoResult Procedimento,
    DateTimeOffset Inicio,
    DateTimeOffset Fim,
    string? MotivoContato,
    string? AnotacoesProfissional,
    decimal ValorTotal,
    string Status,
    string StatusPagamento
);