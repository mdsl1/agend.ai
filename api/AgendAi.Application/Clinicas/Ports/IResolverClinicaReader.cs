namespace AgendAi.Application.Clinicas.Ports;

using AgendAi.Application.Clinicas.Models;

public interface IResolverClinicaReader
{
    Task<DadosClinicaResolvida?> ObterAsync(
        string codigoIdentificador,
        CancellationToken cancellationToken
    );
}