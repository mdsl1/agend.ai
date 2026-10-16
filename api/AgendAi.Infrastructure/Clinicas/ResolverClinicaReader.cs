namespace AgendAi.Infrastructure.Clinicas;

using AgendAi.Application.Clinicas.Ports;
using AgendAi.Application.Clinicas.Models;
using AgendAi.Domain.Organizacoes;
using NHibernate;
using NHibernate.Linq;

public sealed class ResolverClinicaReader : IResolverClinicaReader
{
    private readonly ISession _session;

    public ResolverClinicaReader(
        ISession session
    )
    {
        _session = session;
    }

    public async Task<DadosClinicaResolvida?> ObterAsync(
        string codigoIdentificador,
        CancellationToken cancellationToken
    )
    {
        var clinicaEncontrada = await _session
            .Query<Clinica>()
            .Where(clinica =>
                clinica.CodigoIdentificador == codigoIdentificador
                && clinica.DeletedAt == null
            )
            .Select(clinica => new
            {
                ClinicaUuid = clinica.Uuid,
                NomeClinica = clinica.Nome
            }).SingleOrDefaultAsync(cancellationToken);

        if (clinicaEncontrada is null)
        {
            return null;
        }

        return new DadosClinicaResolvida(
            Uuid: clinicaEncontrada.ClinicaUuid,
            Nome: clinicaEncontrada.NomeClinica
        );
    }
}