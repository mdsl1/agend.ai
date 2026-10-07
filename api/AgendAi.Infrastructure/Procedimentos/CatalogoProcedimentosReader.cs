namespace AgendAi.Infrastructure.Procedimentos;

using AgendAi.Application.Procedimentos.Models;
using AgendAi.Application.Procedimentos.Ports;
using AgendAi.Domain.Profissionais;
using NHibernate;
using NHibernate.Linq;

public sealed class CatalogoProcedimentosReader : ICatalogoProcedimentosReader
{
    private readonly ISession _session;

    public CatalogoProcedimentosReader(ISession session)
    {
        _session = session;
    }

    public async Task<IReadOnlyCollection<DadosProcedimentoCatalogado>> ListarAsync(
        Guid clinicaUuid,
        CancellationToken cancellationToken
    )
    {
        return await _session
            .Query<Procedimento>()
            .Where(procedimento =>
                procedimento.Clinica.Uuid == clinicaUuid
                && procedimento.Clinica.DeletedAt == null
                && procedimento.DeletedAt == null
            )
            .Select(procedimento => new DadosProcedimentoCatalogado(
                procedimento.Clinica.Uuid,
                procedimento.Uuid,
                procedimento.Nome,
                procedimento.DuracaoEstimadaMinutos,
                procedimento.ValorBase
            )).ToListAsync(cancellationToken);
    }
}