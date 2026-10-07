namespace AgendAi.Application.Profissionais.ListarMeusProcedimentos;

using AgendAi.Application.Auth.Permissions;
using AgendAi.Application.Auth.Ports;
using AgendAi.Application.Auth.Services;
using AgendAi.Application.Common.Exceptions;
using AgendAi.Application.Profissionais.Ports;

public sealed class ListarMeusProcedimentosHandler
{
    private readonly IProcedimentosProfissionalReader _reader;
    private readonly IContextUsuarioAtual _context;
    private readonly AutorizacaoService _autorizacaoService;
    
    public ListarMeusProcedimentosHandler(
        IProcedimentosProfissionalReader reader,
        IContextUsuarioAtual context,
        AutorizacaoService autorizacaoService
    )
    {
        _reader = reader;
        _context = context;
        _autorizacaoService = autorizacaoService;
    }

    public async Task<ListarMeusProcedimentosResult> HandleAsync(
        ListarMeusProcedimentosQuery query,
        CancellationToken cancellationToken
    )
    {
        ArgumentNullException.ThrowIfNull(query);
        
        var usuarioAtual = _context.Obter();

        _autorizacaoService.ExigirPermissao(
            usuarioAtual,
            PermissoesUsuario.ProcedimentosPropriosGerenciar
        );

        var profissionalUuid = usuarioAtual.ProfissionalUuid
            ?? throw new AcessoNegadoException();

        if (profissionalUuid == Guid.Empty)
        {
            throw new AcessoNegadoException();
        }

        var dados = await _reader.ListarAsync(
            profissionalUuid, 
            cancellationToken
        ) ?? throw new RecursoNaoEncontradoException(
            codigo: "profissional_nao_encontrado",
            mensagem: "O profissional autenticado não foi encontrado."
        );

        var procedimentos = dados.Procedimentos
            .Select(procedimento => new MeuProcedimentoResult(
                ProfissionalProcedimentoUuid: procedimento.ProfissionalProcedimentoUuid,
                ProcedimentoUuid: procedimento.ProcedimentoUuid,
                Nome: procedimento.Nome,
                ValorEfetivo: procedimento.Valor,
                DuracaoEfetivaMinutos: procedimento.DuracaoMinutos
            )).ToArray();

        return new ListarMeusProcedimentosResult(Procedimentos: procedimentos);
    }
}
