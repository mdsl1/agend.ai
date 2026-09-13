namespace AgendAi.Application.Clientes.Models;

using AgendAi.Domain.Clientes;
using AgendAi.Domain.Organizacoes;

public sealed record DadosResolucaoCliente(
    Clinica Clinica,
    Cliente? ClientePorTelefone,
    Cliente? ClientePorTelegram
);