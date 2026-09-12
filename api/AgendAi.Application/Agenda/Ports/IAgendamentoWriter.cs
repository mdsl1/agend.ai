namespace AgendAi.Application.Agenda.Ports;

using AgendAi.Domain.Agendamentos;

public interface IAgendamentoWriter
{
    Task InserirAsync(
        Agendamento agendamento,
        CancellationToken cancellationToken
    );

    Task AtualizarAsync(
        Agendamento agendamento,
        CancellationToken cancellationToken
    );
}