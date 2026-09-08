namespace AgendAi.Application.Agenda.Ports;

using AgendAi.Application.Agenda.Models;

public interface IDisponibilidadeExternaGateway
{
    Task<ResultadoDisponibilidadeExterna> ConsultarAsync (
        ConsultaDisponibilidadeExterna consulta,
        Uri urlWebhookAgenda,
        CancellationToken cancellationToken
    );
}