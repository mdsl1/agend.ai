namespace AgendAi.Application.Agenda.ConsultarAgendamento;

public sealed record ProcedimentoAgendamentoResult(
    Guid ProcedimentoUuid,
    string NomeProcedimento,
    int DuracaoMinutos
);