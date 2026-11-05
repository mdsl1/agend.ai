namespace AgendAi.Application.Agenda.Ports;
using AgendAi.Application.Agenda.Models;

public interface IAgendamentoStatusWriter
{
    Task <ResultadoAlteracaoStatusAgendamento?> AlterarAsync(
        Guid agendamentoUuid,
        Guid clinicaUuid,
        string novoStatus,
        DateTimeOffset alteradoEm,
        CancellationToken cancellationToken
    );
}