namespace AgendAi.API.Contracts.Clientes;

public sealed record ClienteResponse(
    Guid Uuid,
    string Nome,
    string Telefone,
    string? Email,
    DateOnly? DataNascimento,
    string? Genero
);