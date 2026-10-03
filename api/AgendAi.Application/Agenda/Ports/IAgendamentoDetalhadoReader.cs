namespace AgendAi.Application.Agenda.Ports;

using AgendAi.Application.Agenda.Models;

public interface IAgendamentoDetalhadoReader
{
    Task<DadosAgendamentoDetalhado?> ConsultarAsync(
        Guid agendamentoUuid,
        Guid clinicaUuid,
        CancellationToken cancellationToken
    );
}