namespace AgendAi.Application.Agenda.Ports;

using AgendAi.Application.Agenda.Models;

public interface IExclusaoEventoAgendaExternaGateway
{
    Task<ResultadoExclusaoEventoAgendaExterna> ExcluirAsync(
        ExclusaoEventoAgendaExterna exclusao,
        Uri urlWebhookAgenda,
        CancellationToken cancellationToken
    );
}