namespace AgendAi.Application.Agenda.Ports;

using AgendAi.Application.Agenda.Models;
using AgendAi.Domain.Usuarios;

public interface ICriacaoAgendamentoExternoGateway
{
    Task<ResultadoCriacaoAgendamentoExterno> CriarAsync(
        CriacaoAgendamentoExterno create,
        Uri urlWebhookAgenda,
        CancellationToken cancellationToken
    );
}