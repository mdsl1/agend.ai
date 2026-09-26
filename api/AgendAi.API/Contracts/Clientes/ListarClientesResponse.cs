namespace AgendAi.API.Contracts.Clientes;

public sealed record ListarClientesResponse(
    IReadOnlyCollection<ClienteResponse> Clientes
);