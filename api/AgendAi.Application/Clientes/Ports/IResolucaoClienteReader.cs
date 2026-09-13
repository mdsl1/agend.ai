namespace AgendAi.Application.Clientes.Ports;

using AgendAi.Application.Clientes.Models;

public interface IResolucaoClienteReader
{
    Task<DadosResolucaoCliente?> ObterAsync(
        Guid clinicaUuid,
        string telefone,
        string? telegramUserId,
        CancellationToken cancellationToken
    );
}