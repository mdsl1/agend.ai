namespace AgendAi.Infrastructure.Agenda;

using AgendAi.Application.Agenda.Models;
using AgendAi.Application.Agenda.Ports;
using AgendAi.Domain.Profissionais;
using NHibernate;
using NHibernate.Linq;

public sealed class ProfissionalAgendaReader : IProfissionalAgendaReader
{
    private readonly ISession _session;

    public ProfissionalAgendaReader(ISession session)
    {
        _session = session;
    }

    public async Task<DadosAgendaProfissional?> ObterPorUuidAsync(
        Guid profissionalUuid,
        CancellationToken cancellationToken
    )
    {
        return await 
        _session.Query<Profissional>()
        .Where(profissional => 
        profissional.Uuid == profissionalUuid 
        && profissional.Clinica.DeletedAt == null
        && profissional.Usuario.DeletedAt == null)
        .Select(profissional => new DadosAgendaProfissional (
            profissional.Uuid,
            profissional.Clinica.Uuid,
            profissional.IdGoogleCalendar,
            profissional.Clinica.WebhookCalendar
        ))
        .SingleOrDefaultAsync(cancellationToken);
    }
}