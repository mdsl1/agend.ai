namespace AgendAi.Application.Clientes.ListarClientes;

using AgendAi.Application.Auth.Permissions;
using AgendAi.Application.Auth.Ports;
using AgendAi.Application.Auth.Services;
using AgendAi.Application.Clientes.Ports;

public sealed class ListarClientesHandler
{
    private readonly IClientesReader _reader;
    private readonly IContextUsuarioAtual _context;
    private readonly AutorizacaoService _autorizacaoService;

    public ListarClientesHandler(
        IClientesReader reader,
        IContextUsuarioAtual context,
        AutorizacaoService autorizacaoService
    )
    {
        _reader = reader;
        _context = context;
        _autorizacaoService = autorizacaoService;
    }

    public async Task<ListarClientesResult> HandleAsync(
        ListarClientesQuery query,
        CancellationToken cancellationToken
    )
    {
        ArgumentNullException.ThrowIfNull(query);

        var usuarioAtual = _context.Obter();

        _autorizacaoService.ExigirPermissao(
            usuarioAtual,
            PermissoesUsuario.ClientesClinicaGerenciar
        );

        var dadosCliente = await _reader.ListarAsync(
            usuarioAtual.ClinicaUuid,
            cancellationToken
        );

        var clientes = dadosCliente
            .Select(cliente => new ClienteResult(
                Uuid: cliente.ClienteUuid,
                Nome: cliente.Nome,
                Telefone: cliente.Telefone,
                Email: cliente.Email,
                DataNascimento: cliente.DataNascimento,
                Genero: cliente.Genero
            ))
            .OrderBy(cliente => cliente.Nome, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return new ListarClientesResult(clientes);
    }
}