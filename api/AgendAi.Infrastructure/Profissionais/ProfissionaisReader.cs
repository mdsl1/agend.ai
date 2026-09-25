namespace AgendAi.Infrastructure.Profissionais;

using AgendAi.Application.Profissionais.Ports;
using AgendAi.Application.Profissionais.Models;
using AgendAi.Domain.Profissionais;
using NHibernate;
using NHibernate.Linq;


public sealed class ProfissionaisReader : IProfissionaisReader
{
    private readonly ISession _session;

    public ProfissionaisReader(ISession session)
    {
        _session = session;
    }

    public async Task<IReadOnlyCollection<DadosProfissionalAgendavel>> ListarAsync (
        Guid clinicaUuid,
        Guid? especialidadeUuid,
        Guid? profissionalUuid,
        CancellationToken cancellationToken
    )
    {
        var query = _session.Query<Profissional>()
            .Where(profissional => 
                profissional.Clinica.Uuid == clinicaUuid
                && profissional.DeletedAt == null
                && profissional.Clinica.DeletedAt == null
                && profissional.Usuario.DeletedAt == null
                && profissional.IdGoogleCalendar != null 
                && profissional.IdGoogleCalendar != ""
                && (
                    profissional.Especialidade == null
                    || profissional.Especialidade.DeletedAt == null
                )
            );

        if (especialidadeUuid.HasValue)
        {
            query = query.Where(profissional =>
                profissional.Especialidade != null
                && profissional.Especialidade.Uuid == especialidadeUuid.Value
            );
        }

        if (profissionalUuid.HasValue)
        {
            query = query.Where(profissional => profissional.Uuid == profissionalUuid.Value);
        }

        return await query
            .Select(profissional => new DadosProfissionalAgendavel(
                profissional.Uuid,
                profissional.Usuario.Nome,
                profissional.Prefixo,
                profissional.Especialidade != null
                    ? profissional.Especialidade.Uuid
                    : (Guid?)null,
                profissional.Especialidade != null
                    ? profissional.Especialidade.Nome
                    : null
            )).ToListAsync(cancellationToken);
    }
}