namespace AgendAi.Application.Procedimentos.ListarCatalogoProcedimentos;

using AgendAi.Application.Auth.Permissions;
using AgendAi.Application.Auth.Ports;
using AgendAi.Application.Auth.Services;
using AgendAi.Application.Common.Exceptions;
using AgendAi.Application.Procedimentos.Ports;

public sealed class ListarCatalogoProcedimentosHandler
{
    private readonly ICatalogoProcedimentosReader _reader;
    private readonly IContextUsuarioAtual _context;
    private readonly AutorizacaoService _autorizacaoService;

    public ListarCatalogoProcedimentosHandler(
        ICatalogoProcedimentosReader reader,
        IContextUsuarioAtual context,
        AutorizacaoService autorizacaoService
    )
    {
        _reader = reader;
        _context = context;
        _autorizacaoService = autorizacaoService;
    }

    public async Task<ListarCatalogoProcedimentosResult> HandleAsync(
        ListarCatalogoProcedimentosQuery query,
        CancellationToken cancellationToken
    )
    {
        ArgumentNullException.ThrowIfNull(query);

        var usuarioAtual = _context.Obter();

        if (
            !_autorizacaoService.PossuiPermissao(usuarioAtual, PermissoesUsuario.ProcedimentosClinicaGerenciar)
            && !_autorizacaoService.PossuiPermissao(usuarioAtual, PermissoesUsuario.ProcedimentosPropriosGerenciar)
        )
        {
            throw new AcessoNegadoException();
        }

        var dados = await _reader.ListarAsync(
            usuarioAtual.ClinicaUuid,
            cancellationToken
        );

        var procedimentos = dados
            .Select(procedimento => new ProcedimentoCatalogadoResult(
                ProcedimentoUuid: procedimento.ProcedimentoUuid,
                NomeProcedimento: procedimento.NomeProcedimento,
                DuracaoEstimadaMinutos: procedimento.DuracaoEstimadaMinutos,
                ValorBase: procedimento.ValorBase
            ))
            .OrderBy(procedimento => 
                procedimento.NomeProcedimento,
                StringComparer.OrdinalIgnoreCase
            ).ToArray();

        return new ListarCatalogoProcedimentosResult(procedimentos);

    }
}


