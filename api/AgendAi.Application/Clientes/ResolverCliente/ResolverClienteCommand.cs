namespace AgendAi.Application.Clientes.ResolverCliente;

public sealed record ResolverClienteCommand(
    Guid ClinicaUuid,
    string Nome,
    string Telefone,
    string? TelegramUserId
);