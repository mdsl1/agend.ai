namespace AgendAi.Application.Agenda.Ports;
using AgendAi.Application.Agenda.Models;

public interface IAgendasReader
{
    Task<IReadOnlyCollection<DadosAgendaDisponivel>> ListarPorClinicaAsync (
        Guid clinicaUuid,
        CancellationToken cancellationToken
    );
}
