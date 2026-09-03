namespace AgendAi.Application.Agenda.Ports;

using AgendAi.Application.Agenda.Models;

public interface IAgendamentoAgendaReader
{
    Task<IReadOnlyCollection<DadosAgendamentoAgenda>> ListarPorIdsEventosExternosAsync(
        Guid profissionalUuid,
        IReadOnlyCollection<string> idsEventosExternos,
        CancellationToken cancellationToken
    );
}