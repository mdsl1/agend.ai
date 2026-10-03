namespace AgendAi.Application.Agenda.ConsultarAgendamento;

using AgendAi.Application.Agenda.Ports;
using AgendAi.Application.Auth.Permissions;
using AgendAi.Application.Auth.Ports;
using AgendAi.Application.Auth.Services;
using AgendAi.Application.Common.Exceptions;

public sealed class ConsultarAgendamentoHandler
{
    private readonly IAgendamentoDetalhadoReader _reader;
    private readonly IContextUsuarioAtual _context;
    private readonly AutorizacaoService _autorizacaoService;

    public ConsultarAgendamentoHandler(
        IAgendamentoDetalhadoReader reader,
        IContextUsuarioAtual context,
        AutorizacaoService autorizacaoService
    )
    {
        _reader = reader;
        _context = context;
        _autorizacaoService = autorizacaoService;
    }

    public async Task<ConsultarAgendamentoResult> HandleAsync(
        ConsultarAgendamentoQuery query,
        CancellationToken cancellationToken
    )
    {
        ArgumentNullException.ThrowIfNull(query);

        ValidarQuery(query);

        var usuarioAtual = _context.Obter();

        var dados = await _reader.ConsultarAsync(
            query.AgendamentoUuid,
            usuarioAtual.ClinicaUuid,
            cancellationToken
        ) ?? throw new RecursoNaoEncontradoException(
            "agendamento_nao_encontrado",
            "O agendamento informado não foi encontrado."
        );

        _autorizacaoService.ExigirAcessoAoProfissional(
            usuario: usuarioAtual,
            clinicaUuid: dados.ClinicaUuid,
            profissionalUuid: dados.ProfissionalUuid,
            permissaoPropria: PermissoesUsuario.AgendamentosPropriosGerenciar,
            permissaoClinica: PermissoesUsuario.AgendamentosClinicaGerenciar
        );

        return new ConsultarAgendamentoResult(
            Uuid: dados.AgendamentoUuid,
            Cliente: new ClienteAgendamentoResult(
                dados.ClienteUuid,
                dados.NomeCliente,
                dados.Telefone
            ),
            Profissional: new ProfissionalAgendamentoResult(
                dados.ProfissionalUuid,
                dados.NomeExibicaoProfissional
            ),
            Procedimento: new ProcedimentoAgendamentoResult(
                dados.ProcedimentoUuid,
                dados.NomeProcedimento,
                dados.DuracaoMinutos
            ),
            Inicio: dados.Inicio,
            Fim: dados.Fim,
            MotivoContato: dados.MotivoContato,
            AnotacoesProfissional: dados.AnotacoesProfissional,
            ValorTotal: dados.ValorTotal,
            Status: dados.Status,
            StatusPagamento: dados.StatusPagamento
        );
    }

    private static void ValidarQuery(ConsultarAgendamentoQuery query)
    {
        if (query.AgendamentoUuid == Guid.Empty)
        {
            throw new ValidacaoException(
                "agendamento_uuid_invalido",
                "O UUID do agendamento é obrigatório."
            );
        }
    }
}