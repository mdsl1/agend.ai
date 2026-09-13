namespace AgendAi.API.Contracts.Clientes;

public sealed record ResolverClienteResponse(
    Guid ClienteUuid,
    bool Criado,
    string Nome,
    string Telefone
);