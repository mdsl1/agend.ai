namespace AgendAi.Application.Agenda.AlterarStatusAgendamento;

public sealed record AlterarStatusAgendamentoCommand(
    Guid AgendamentoUuid,
    string NovoStatus
);