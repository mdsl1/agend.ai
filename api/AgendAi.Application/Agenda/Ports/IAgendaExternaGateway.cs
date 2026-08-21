namespace AgendAi.Application.Agenda.Ports;
using AgendAi.Application.Agenda.Models;

public interface IAgendaExternaGateway
{
    Task<IReadOnlyCollection<EventoAgendaExterna>> ConsultarAsync (
        Guid profissionalUuid,
        string idAgendaExterna,
        Uri urlWebhookAgenda,
        DateTimeOffset inicio,
        DateTimeOffset fim,
        CancellationToken cancellationToken
    );
}