namespace AgendAi.Application.Overview.Ports;

using AgendAi.Application.Overview.Models;

public interface IResumoAgendaReader
{
    Task<DadosResumoAgenda> ConsultarAsync(
        Guid clinicaUuid,
        Guid profissionalUuid,
        DateTimeOffset inicioDia,
        DateTimeOffset fimDia,
        DateTimeOffset agora,
        CancellationToken cancellationToken
    );
}