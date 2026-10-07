namespace AgendAi.Application.Procedimentos.Ports;

using AgendAi.Application.Procedimentos.Models;

public interface ICatalogoProcedimentosReader
{
    Task<IReadOnlyCollection<DadosProcedimentoCatalogado>> ListarAsync(
        Guid clinicaUuid,
        CancellationToken cancellationToken
    );
}