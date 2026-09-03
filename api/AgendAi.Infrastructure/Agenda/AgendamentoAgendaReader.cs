namespace AgendAi.Infrastructure.Agenda;

using AgendAi.Application.Agenda.Models;
using AgendAi.Application.Agenda.Ports;
using AgendAi.Domain.Agendamentos;
using NHibernate;
using NHibernate.Linq;

public sealed class AgendamentoAgendaReader : IAgendamentoAgendaReader
{
    private readonly ISession _session;

    public AgendamentoAgendaReader(ISession session)
    {
        _session = session;
    }

    public async Task<IReadOnlyCollection<DadosAgendamentoAgenda>> ListarPorIdsEventosExternosAsync(
        Guid profissionalUuid,
        IReadOnlyCollection<string> idsEventosExternos,
        CancellationToken cancellationToken
    )
    {
        var ids = idsEventosExternos
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Distinct()
            .ToArray();

        if(ids.Length == 0)
        {
            return Array.Empty<DadosAgendamentoAgenda>();
        }

        return await _session.Query<Agendamento>()
            .Where(agendamento =>
                agendamento.Profissional.Uuid == profissionalUuid
                && agendamento.DeletedAt == null
                && agendamento.IdEventGoogleCalendar != null
                && ids.Contains(agendamento.IdEventGoogleCalendar!))
            .Select(agendamento => new DadosAgendamentoAgenda (
                agendamento.IdEventGoogleCalendar!,
                agendamento.Uuid
            ))
            .ToListAsync(cancellationToken);
    }
}
