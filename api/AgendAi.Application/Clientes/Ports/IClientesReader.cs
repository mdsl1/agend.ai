namespace AgendAi.Application.Clientes.Ports;

using AgendAi.Application.Clientes.Models;

public interface IClientesReader
{
    Task<IReadOnlyCollection<DadosCliente>> ListarAsync(
        Guid clinicaUuid,
        CancellationToken cancellationToken
    );
}