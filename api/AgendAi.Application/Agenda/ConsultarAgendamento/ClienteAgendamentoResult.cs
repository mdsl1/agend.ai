namespace AgendAi.Application.Agenda.ConsultarAgendamento;

public sealed record ClienteAgendamentoResult(
    Guid ClienteUuid,
    string NomeCliente,
    string Telefone
);