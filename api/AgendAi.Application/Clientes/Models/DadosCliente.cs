namespace AgendAi.Application.Clientes.Models;

public sealed record DadosCliente(
    Guid ClienteUuid,
    string Nome,
    string Telefone,
    string? Email,
    DateOnly? DataNascimento,
    string? Genero
);