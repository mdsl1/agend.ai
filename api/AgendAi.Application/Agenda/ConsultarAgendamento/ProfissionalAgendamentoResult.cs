namespace AgendAi.Application.Agenda.ConsultarAgendamento;

public sealed record ProfissionalAgendamentoResult(
    Guid ProfissionalUuid,
    string NomeExibicao
);