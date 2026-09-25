namespace AgendAi.Application.Agenda.CriarAgendamento;

using AgendAi.Application.Agenda.Ports;
using AgendAi.Application.Common.Exceptions;
using AgendAi.Application.Agenda.Services;
using AgendAi.Domain.Agendamentos;
using AgendAi.Application.Agenda.Models;
using AgendAi.Application.Auth.Permissions;
using AgendAi.Application.Auth.Ports;
using AgendAi.Application.Auth.Services;

public sealed class CriarAgendamentoHandler
{
    private const int TamanhoMaximoIdempotencyKey = 200;

    private readonly ICriacaoAgendamentoReader _criacaoAgendamentoReader;
    private readonly IAgendamentoWriter _agendamentoWriter;
    private readonly ICriacaoAgendamentoExternoGateway _criacaoAgendamentoExternoGateway;
    private readonly VerificarDisponibilidadeService _verificarDisponibilidadeService;
    private readonly IContextUsuarioAtual _context;
    private readonly AutorizacaoService _autorizacaoService;

    public CriarAgendamentoHandler(
        ICriacaoAgendamentoReader criacaoAgendamentoReader,
        IAgendamentoWriter agendamentoWriter,
        ICriacaoAgendamentoExternoGateway criacaoAgendamentoExternoGateway,
        VerificarDisponibilidadeService verificarDisponibilidadeService,
        IContextUsuarioAtual context,
        AutorizacaoService autorizacaoService
    )
    {
        _criacaoAgendamentoReader = criacaoAgendamentoReader;
        _agendamentoWriter = agendamentoWriter;
        _criacaoAgendamentoExternoGateway = criacaoAgendamentoExternoGateway;
        _verificarDisponibilidadeService = verificarDisponibilidadeService;
        _context = context;
        _autorizacaoService = autorizacaoService;
    }

    public async Task<CriarAgendamentoResult> HandleAsync(
        CriarAgendamentoCommand command,
        CancellationToken cancellationToken
    )
    {
        ArgumentNullException.ThrowIfNull(command);

        ValidarCommand(command);

        var usuarioAtual = _context.Obter();

        var dados = await _criacaoAgendamentoReader.ObterAsync(
            command.ClienteUuid,
            command.ProfissionalProcedimentoUuid,
            cancellationToken
        ) ?? throw new RecursoNaoEncontradoException(
            codigo: "dados_criacao_agendamento_nao_encontrados",
            mensagem: "O cliente ou a associação entre profissional e procedimento não foi encontrada."
        );

        _autorizacaoService.ExigirAcessoAoProfissional(
            usuario: usuarioAtual,
            clinicaUuid: dados.ProfissionalProcedimento.Clinica.Uuid,
            profissionalUuid: dados.ProfissionalProcedimento.Profissional.Uuid,
            permissaoPropria: PermissoesUsuario.AgendamentosPropriosGerenciar,
            permissaoClinica: PermissoesUsuario.AgendamentosClinicaGerenciar
        );

        var idempotencyKey = command.IdempotencyKey.Trim();

        var agendamentoExistente = await _criacaoAgendamentoReader.ObterPorChaveIdempotenciaAsync(
            dados.ProfissionalProcedimento.Clinica.Uuid,
            idempotencyKey,
            cancellationToken
        );

        if (agendamentoExistente is not null)
        {
            return TratarRequestRepetida(
                agendamentoExistente,
                dados,
                command
            );
        }

        var dadosDisponibilidade = CriarDadosDisponibilidade(dados);

        var resultDisponibilidade = await _verificarDisponibilidadeService.VerificarAsync(
            dadosDisponibilidade,
            command.Inicio,
            limite: 1,
            cancellationToken
        );

        if (!resultDisponibilidade.Disponivel)
        {
            throw new ConflitoException(
                codigo: "horario_indisponivel",
                mensagem: "O horário solicitado não está mais disponível."
            );
        }

        var fim = command.Inicio.AddMinutes(dados.ProfissionalProcedimento.DuracaoMinutos);

        var agendamento = Agendamento.CriarPendente(
            chaveIdempotencia: idempotencyKey,
            dados.Cliente,
            dados.ProfissionalProcedimento,
            command.Inicio,
            fim,
            command.MotivoContato
        );

        await _agendamentoWriter.InserirAsync(
            agendamento,
            cancellationToken
        );

        try
        {
            var createExterno = CriarDadosIntegracao(
                agendamento,
                dados
            );

            var webhookUri = new Uri(
                dadosDisponibilidade.UrlWebhookAgenda!,
                UriKind.Absolute
            );

            var resultExterno = await _criacaoAgendamentoExternoGateway.CriarAsync(
                createExterno,
                webhookUri,
                cancellationToken
            );

            agendamento.ConfirmarIntegracao(resultExterno.IdEventoExterno);

            await _agendamentoWriter.AtualizarAsync(
                agendamento,
                cancellationToken
            );
        }
        catch (IntegracaoExternaException)
        {
            agendamento.RegistrarFalhaIntegracao();

            await _agendamentoWriter.AtualizarAsync(
                agendamento,
                cancellationToken
            );

            throw;
        }

        return MapearResult(agendamento);
    }

    private static void ValidarCommand(CriarAgendamentoCommand command)
    {
        if (command.ClienteUuid == Guid.Empty)
        {
            throw new ValidacaoException(
                codigo: "cliente_uuid_invalido",
                mensagem: "O UUID do cliente é obrigatório."
            );
        }

        if (command.ProfissionalProcedimentoUuid == Guid.Empty)
        {
            throw new ValidacaoException(
                codigo: "profissional_procedimento_uuid_invalido",
                mensagem: "O UUID de associação entre profissional e procedimento é obrigatório."
            );
        }

        if (command.Inicio == default)
        {
            throw new ValidacaoException(
                codigo: "inicio_obrigatorio",
                mensagem: "O início do agendamento é obrigatório."
            );
        }

        if (string.IsNullOrWhiteSpace(command.IdempotencyKey))
        {
            throw new ValidacaoException(
                codigo: "idempotency_key_obrigatoria",
            mensagem: "O header `Idempotency-Key` é obrigatório."
            );
        }

        if (command.IdempotencyKey.Trim().Length > TamanhoMaximoIdempotencyKey)
        {
            throw new ValidacaoException(
                codigo: "idempotency_key_invalida",
                mensagem: $"O header `Idempotency-Key` deve possuir no máximo {TamanhoMaximoIdempotencyKey} caracteres."
            );
        }
    }

    private static DadosDisponibilidadeProfissional CriarDadosDisponibilidade(DadosCriacaoAgendamento dados)
    {
        var vinculo = dados.ProfissionalProcedimento;

        return new DadosDisponibilidadeProfissional(
            ProfissionalUuid: vinculo.Profissional.Uuid,
            ProfissionalProcedimentoUuid: vinculo.Uuid,
            ClinicaUuid: vinculo.Clinica.Uuid,
            DuracaoMinutos: vinculo.DuracaoMinutos,
            IdAgendaExterna: vinculo.Profissional.IdGoogleCalendar,
            UrlWebhookAgenda: vinculo.Clinica.WebhookCalendar,
            JanelasAtendimento: dados.JanelasAtendimento
        );
    }

    private static CriacaoAgendamentoExterno CriarDadosIntegracao(
        Agendamento agendamento,
        DadosCriacaoAgendamento dados
    )
    {
        var vinculo = dados.ProfissionalProcedimento;

        return new CriacaoAgendamentoExterno(
            AgendamentoUuid: agendamento.Uuid,
            Inicio: new DateTimeOffset(agendamento.TimeDateInicio),
            Fim: new DateTimeOffset(agendamento.TimeDateFim),
            MotivoContato: agendamento.MotivoContato,
            ProfissionalUuid: vinculo.Profissional.Uuid,
            IdAgendaExterna: vinculo.Profissional.IdGoogleCalendar!,
            NomeProcedimento: vinculo.Procedimento.Nome,
            NomeCliente: dados.Cliente.Nome
        );
    }

    private static CriarAgendamentoResult TratarRequestRepetida(
        Agendamento agendamento,
        DadosCriacaoAgendamento dados,
        CriarAgendamentoCommand command
    )
    {
        if (!RepresentaMesmaSolicitacao(agendamento, dados, command))
        {
            throw new ConflitoException(
                codigo: "idempotency_key_reutilizada",
                mensagem: "A Idempotency-Key informada já foi utilizada em outra solicitação."
            );
        }

        if (agendamento.Status == Agendamento.StatusAgendado)
        {
            return MapearResult(agendamento);
        }

        if (agendamento.Status == Agendamento.StatusPendenteIntegracao)
        {
            throw new ConflitoException(
                codigo: "agendamento_pendente_integracao",
                mensagem: "A operação associada à essa Idempotency-Key ainda está pendente."
            );
        }

        throw new ConflitoException(
            codigo: "agendamento_com_falha_integracao",
            mensagem: "A operação associada à essa Idempotency-Key terminou com falha."
        );
    }

    private static bool RepresentaMesmaSolicitacao(
        Agendamento agendamento,
        DadosCriacaoAgendamento dados,
        CriarAgendamentoCommand command
    )
    {
        var vinculo = dados.ProfissionalProcedimento;

        var fimEsperado = command.Inicio.AddMinutes(vinculo.DuracaoMinutos);

        return agendamento.Cliente.Uuid == command.ClienteUuid
            && agendamento.Profissional.Uuid == vinculo.Profissional.Uuid
            && agendamento.Procedimento?.Uuid == vinculo.Procedimento.Uuid
            && agendamento.TimeDateInicio == command.Inicio.UtcDateTime
            && agendamento.TimeDateFim == fimEsperado.UtcDateTime
            && string.Equals(
                agendamento.MotivoContato,
                string.IsNullOrWhiteSpace( command.MotivoContato) ? null : command.MotivoContato.Trim(),
                StringComparison.Ordinal
            );
    }

    private static CriarAgendamentoResult MapearResult( Agendamento agendamento)
    {
        var procedimento = agendamento.Procedimento 
            ?? throw new InvalidOperationException("O agendamento não possui um procedimento.");
        
        var nomeProfissionalExibicao = string.IsNullOrWhiteSpace(agendamento.Profissional.Prefixo) 
            ? agendamento.Profissional.Usuario.Nome.Trim()
            : $"{ agendamento.Profissional.Prefixo.Trim() } { agendamento.Profissional.Usuario.Nome.Trim() }";

        return new CriarAgendamentoResult(
            AgendamentoUuid: agendamento.Uuid,
            Inicio: new DateTimeOffset(agendamento.TimeDateInicio),
            Fim: new DateTimeOffset(agendamento.TimeDateFim),
            NomeCliente: agendamento.Cliente.Nome,
            NomeProfissional: nomeProfissionalExibicao,
            NomeProcedimento: procedimento.Nome,
            ValorTotal: agendamento.ValorTotal,
            Status: agendamento.Status
        );
    }
}