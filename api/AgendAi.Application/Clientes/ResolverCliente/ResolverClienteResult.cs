namespace AgendAi.Application.Clientes.ResolverCliente;

public sealed record ResolverClienteResult(
    Guid ClienteUuid,
    bool Criado,
    string Nome,
    string Telefone
);