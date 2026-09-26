namespace AgendAi.Application.Clientes.ListarClientes;

public sealed record ClienteResult(
    Guid Uuid,
    string Nome,
    string Telefone,
    string? Email,
    DateOnly? DataNascimento,
    string? Genero
);