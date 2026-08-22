using AgendAi.Application.Agenda.Ports;
using AgendAi.Application.Agenda.Models;
using NHibernate;
using NHibernate.Linq;
using AgendAi.Domain.Profissionais;

namespace AgendAi.Infrastructure.Agenda;

public sealed class AgendasReader : IAgendasReader
{
    private readonly ISession _session;

    public AgendasReader(ISession session)
    {
        _session = session;
    }

    public async Task<IReadOnlyCollection<DadosAgendaDisponivel>> ListarPorClinicaAsync (
        Guid clinicaUuid,
        CancellationToken cancellationToken
    )
    {
        return await
        _session.Query<Profissional>()
        .Where(profissional => 
        profissional.Clinica.Uuid == clinicaUuid
        && profissional.Clinica.DeletedAt == null
        && profissional.Usuario.DeletedAt == null
        && profissional.IdGoogleCalendar != null 
        && profissional.IdGoogleCalendar != "")
        .Select(profissional => new DadosAgendaDisponivel (
            profissional.Uuid,
            profissional.Usuario.Nome,
            profissional.Prefixo
        ))
        .ToListAsync(cancellationToken);
    }
}