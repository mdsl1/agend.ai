namespace AgendAi.API.Contracts.Clientes;

public sealed record ResolverClienteRequest(
    string Nome,
    string Telefone,
    string? TelegramUserId
);