namespace AgendAi.Application.Agenda.Ports;

using AgendAi.Application.Agenda.Models;

public interface ICriacaoIndisponibilidadeExternaGateway
{
    Task<ResultadoCriacaoIndisponibilidadeExterna> CriarAsync(
        CriacaoIndisponibilidadeExterna criacao,
        Uri urlWebhookAgenda,
        CancellationToken cancellationToken
    );
}