namespace AgendAi.Application.Clientes.ListarClientes;

public sealed record ListarClientesResult(
    IReadOnlyCollection<ClienteResult> Clientes
);