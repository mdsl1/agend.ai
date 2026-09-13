namespace AgendAi.Application.Clientes.Ports;

using AgendAi.Domain.Clientes;

public interface IClienteWriter
{
    Task InserirAsync(
        Cliente cliente,
        CancellationToken cancellationToken
    );
    Task AtualizarAsync(
        Cliente cliente,
        CancellationToken cancellationToken
    );
}