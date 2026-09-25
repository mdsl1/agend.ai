namespace AgendAi.Application.Clientes.ResolverCliente;

public sealed record ResolverClienteCommand(
    string Nome,
    string Telefone,
    string? TelegramUserId
);