namespace AgendAi.Application.Profissionais.ListarProcedimentosProfissional;

using AgendAi.Application.Common.Exceptions;
using AgendAi.Application.Profissionais.ListarProcedimentosProfissional.Ports;
using AgendAi.Application.Auth.Permissions;
using AgendAi.Application.Auth.Ports;
using AgendAi.Application.Auth.Services;

public sealed class ListarProcedimentosProfissionalHandler
{
    private readonly IProcedimentosProfissionalReader _procedimentosProfissionalReader;
    private readonly IContextUsuarioAtual _context;
    private readonly AutorizacaoService _autorizacaoService;

    public ListarProcedimentosProfissionalHandler(
        IProcedimentosProfissionalReader procedimentosProfissionalReader,
        IContextUsuarioAtual context,
        AutorizacaoService autorizacaoService
    )
    {
        _procedimentosProfissionalReader = procedimentosProfissionalReader;
        _context = context;
        _autorizacaoService = autorizacaoService;
    }

    public async Task<ListarProcedimentosProfissionalResult> HandleAsync(
        ListarProcedimentosProfissionalQuery query,
        CancellationToken cancellationToken
    )
    {
        ArgumentNullException.ThrowIfNull(query);

        ValidarQuery(query);

        var usuarioAtual = _context.Obter();

        var dados = await _procedimentosProfissionalReader.ListarAsync(
            query.ProfissionalUuid,
            cancellationToken
        ) ?? throw new RecursoNaoEncontradoException(
            codigo: "profissional_nao_encontrado",
            mensagem: "O profissional informado não foi encontrado."
        );

        _autorizacaoService.ExigirAcessoAoProfissional(
            usuario: usuarioAtual,
            clinicaUuid: dados.ClinicaUuid,
            profissionalUuid: dados.ProfissionalUuid,
            permissaoPropria: PermissoesUsuario.ProcedimentosPropriosGerenciar,
            permissaoClinica: PermissoesUsuario.ProcedimentosClinicaGerenciar
        );

        var procedimentos = dados.Procedimentos
            .Select(procedimento => new ProcedimentoProfissionalResult(
                ProfissionalProcedimentoUuid: procedimento.ProfissionalProcedimentoUuid,
                ProcedimentoUuid: procedimento.ProcedimentoUuid,
                Nome: procedimento.Nome,
                ValorEfetivo: procedimento.Valor,
                DuracaoEfetivaMinutos: procedimento.DuracaoMinutos
            )).ToArray();

        return new ListarProcedimentosProfissionalResult(Procedimentos: procedimentos);
    }

    private static void ValidarQuery(ListarProcedimentosProfissionalQuery query)
    {
        if (query.ProfissionalUuid == Guid.Empty)
        {
            throw new ValidacaoException(
                codigo: "profissional_uuid_invalido",
                mensagem: "O UUID do profissional é obrigatório."
            );
        }
    }
}