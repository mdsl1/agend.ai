namespace AgendAi.Application.Agenda.Ports;
using AgendAi.Application.Agenda.Models;

public interface IProfissionalAgendaReader
{
    Task<DadosAgendaProfissional?> ObterPorUuidAsync (
        Guid profissionalUuid, 
        CancellationToken cancellationToken
    );
}