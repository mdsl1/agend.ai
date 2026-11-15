namespace AgendAi.Application.Agenda.ExcluirMinhaIndisponibilidade;

using AgendAi.Application.Agenda.Models;
using AgendAi.Application.Agenda.Ports;
using AgendAi.Application.Auth.Permissions;
using AgendAi.Application.Auth.Ports;
using AgendAi.Application.Auth.Services;
using AgendAi.Application.Common.Exceptions;

public sealed class ExcluirMinhaIndisponibilidadeHandler
{
    private readonly IProfissionalAgendaReader _profissionalAgendaReader;
    private readonly IExclusaoEventoAgendaExternaGateway _exclusaoEventoGateway;
    private readonly IContextUsuarioAtual _context;
    private readonly AutorizacaoService _autorizacaoService;

    public ExcluirMinhaIndisponibilidadeHandler(
        IProfissionalAgendaReader profissionalAgendaReader,
        IExclusaoEventoAgendaExternaGateway exclusaoEventoGateway,
        IContextUsuarioAtual context,
        AutorizacaoService autorizacaoService
    )
    {
        _profissionalAgendaReader = profissionalAgendaReader;
        _exclusaoEventoGateway = exclusaoEventoGateway;
        _context = context;
        _autorizacaoService = autorizacaoService;
    }

    public async Task HandleAsync(
        ExcluirMinhaIndisponibilidadeCommand command,
        CancellationToken cancellationToken
    )
    {
        ArgumentNullException.ThrowIfNull(command);

        var eventoId = ValidarENormalizarEventoId(command.EventoId);

        var usuarioAtual = _context.Obter();

        _autorizacaoService.ExigirPermissao(
            usuarioAtual,
            PermissoesUsuario.IndisponibilidadesPropriasGerenciar
        );

        var profissionalUuid = usuarioAtual.ProfissionalUuid
            ?? throw new AcessoNegadoException();

        if (profissionalUuid == Guid.Empty)
        {
            throw new AcessoNegadoException();
        }

        var dadosProfissional = await _profissionalAgendaReader.ObterPorUuidAsync(
            profissionalUuid,
            cancellationToken
        ) ?? throw new RecursoNaoEncontradoException(
            codigo: "profissional_nao_encontrado",
            mensagem: "O profissional autenticado não foi encontrado."
        );

        _autorizacaoService.ExigirMesmaClinica(
            usuarioAtual,
            dadosProfissional.ClinicaUuid
        );

        var idAgendaExterna = dadosProfissional.IdAgendaExterna;
        if (string.IsNullOrWhiteSpace(idAgendaExterna))
        {
            throw new ConflitoException(
                codigo: "agenda_nao_configurada",
                mensagem: "O profissional ainda não possui uma agenda configurada."
            );
        }

        var urlWebhookAgenda = dadosProfissional.UrlWebhookAgenda;
        if (string.IsNullOrWhiteSpace(urlWebhookAgenda))
        {
            throw new ConflitoException(
                codigo: "webhook_nao_configurado",
                mensagem: "A clínica ainda não possui um webhook de agenda configurado."
            );
        }

        if (
            !Uri.TryCreate(urlWebhookAgenda, UriKind.Absolute, out var webhookUri)
            || (webhookUri.Scheme != Uri.UriSchemeHttp && webhookUri.Scheme != Uri.UriSchemeHttps)
        )
        {
            throw new ConflitoException(
                codigo: "webhook_invalido",
                mensagem: "O webhook de agenda configurado para a clínica é inválido."
            );
        }

        await _exclusaoEventoGateway.ExcluirAsync(
            new ExclusaoEventoAgendaExterna(
                IdEventoExterno: eventoId,
                IdAgendaExterna: idAgendaExterna
            ),
            webhookUri,
            cancellationToken
        );
    }

    private static string ValidarENormalizarEventoId(string? eventoId)
    {
        if (string.IsNullOrWhiteSpace(eventoId))
        {
            throw new ValidacaoException(
                codigo: "evento_id_obrigatorio",
                mensagem: "O identificador do evento de indisponibilidade é obrigatório."
            );
        }

        var idNormalizado = eventoId.Trim();

        if (
            idNormalizado.StartsWith("ag", StringComparison.OrdinalIgnoreCase)
            && Guid.TryParseExact(idNormalizado.AsSpan(2), "N", out _)
        )
        {
            throw new ConflitoException(
                codigo: "exclusao_agendamento_nao_permitida",
                mensagem: "Eventos de agendamento não podem ser removidos pela rota de indisponibilidades."
            );
        }

        return idNormalizado;
    }
}