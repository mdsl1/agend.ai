namespace AgendAi.Application.Agenda.CriarMinhaIndisponibilidade;

using AgendAi.Application.Agenda.Models;
using AgendAi.Application.Agenda.Ports;
using AgendAi.Application.Auth.Permissions;
using AgendAi.Application.Auth.Ports;
using AgendAi.Application.Auth.Services;
using AgendAi.Application.Common.Exceptions;

public sealed class CriarMinhaIndisponibilidadeHandler
{
    private const string MotivoPadrao = "Indisponível";
    private const int TamanhoMaximoMotivo = 150;
    private static readonly string[] FrequenciasPermitidas =
    [
        "Daily",
        "Weekly",
        "Monthly",
        "Yearly"
    ];

    private readonly IProfissionalAgendaReader _profissionalAgendaReader;
    private readonly IAgendaExternaGateway _agendaExternaGateway;
    private readonly ICriacaoIndisponibilidadeExternaGateway _criacaoIndisponibilidadeGateway;
    private readonly IContextUsuarioAtual _context;
    private readonly AutorizacaoService _autorizacaoService;

    public CriarMinhaIndisponibilidadeHandler(
        IProfissionalAgendaReader profissionalAgendaReader,
        IAgendaExternaGateway agendaExternaGateway,
        ICriacaoIndisponibilidadeExternaGateway criacaoIndisponibilidadeGateway,
        IContextUsuarioAtual context,
        AutorizacaoService autorizacaoService
    )
    {
        _profissionalAgendaReader = profissionalAgendaReader;
        _agendaExternaGateway = agendaExternaGateway;
        _criacaoIndisponibilidadeGateway = criacaoIndisponibilidadeGateway;
        _context = context;
        _autorizacaoService = autorizacaoService;
    }

    public async Task<CriarMinhaIndisponibilidadeResult> HandleAsync(
        CriarMinhaIndisponibilidadeCommand command,
        CancellationToken cancellationToken
    )
    {
        ArgumentNullException.ThrowIfNull(command);

        var (motivoNormalizado, frequenciaNormalizada, repetirAteEfetivo) = ValidarENormalizarCommand(command);

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
            "profissional_nao_encontrado",
            "O profissional autenticado não foi encontrado."
        );

        _autorizacaoService.ExigirMesmaClinica(
            usuarioAtual,
            dadosProfissional.ClinicaUuid
        );

        var idAgendaExterna = dadosProfissional.IdAgendaExterna;
        if (string.IsNullOrWhiteSpace(idAgendaExterna))
        {
            throw new ConflitoException(
                "agenda_nao_configurada",
                "O profissional ainda não possui uma agenda configurada."
            );
        }
        var urlWebhookAgenda = dadosProfissional.UrlWebhookAgenda;
        if (string.IsNullOrWhiteSpace(urlWebhookAgenda))
        {
            throw new ConflitoException(
                "webhook_nao_configurado",
                "A clínica ainda não possui um webhook de agenda configurado."
            );
        }

        if (
            !Uri.TryCreate(urlWebhookAgenda, UriKind.Absolute, out var webhookUri)
            || (webhookUri.Scheme != Uri.UriSchemeHttp && webhookUri.Scheme != Uri.UriSchemeHttps)
        )
        {
            throw new ConflitoException(
                "webhook_invalido",
                "O webhook de agenda configurado para a clínica é inválido."
            );
        }

        var eventosNoPeriodo = await _agendaExternaGateway.ConsultarAsync(
            profissionalUuid: profissionalUuid,
            idAgendaExterna: idAgendaExterna,
            urlWebhookAgenda: webhookUri,
            inicio: command.Inicio,
            fim: command.Fim,
            cancellationToken: cancellationToken
        );

        var possuiConflito = eventosNoPeriodo.Any(
            evento => evento.Inicio < command.Fim && evento.Fim > command.Inicio
        );
        if (possuiConflito)
        {
            throw new ConflitoException(
                codigo: "horario_indisponivel",
                mensagem: "Já existe um evento registrado na agenda para o período informado."
            );
        }

        var result = await _criacaoIndisponibilidadeGateway.CriarAsync(
            new CriacaoIndisponibilidadeExterna(
                IdAgendaExterna: idAgendaExterna,
                Inicio: command.Inicio,
                Fim: command.Fim,
                Motivo: motivoNormalizado,
                SeRepete: command.SeRepete,
                Frequencia: frequenciaNormalizada,
                RepetirAte: repetirAteEfetivo
            ),
            webhookUri,
            cancellationToken
        );
        
        return new CriarMinhaIndisponibilidadeResult(
            Id: result.IdEventoExterno,
            Tipo: "indisponibilidade",
            Titulo: motivoNormalizado,
            Inicio: result.Inicio,
            Fim: result.Fim,
            SeRepete: command.SeRepete,
            Frequencia: command.SeRepete ? (result.Frequencia ?? frequenciaNormalizada) : null,
            RepetirAte: command.SeRepete ? (result.RepetirAte ?? repetirAteEfetivo) : null
        );
    }

    private static (string Motivo, string? Frequencia, DateTimeOffset? RepetirAte) ValidarENormalizarCommand(
        CriarMinhaIndisponibilidadeCommand command
    )
    {
        if (command.Inicio == default || command.Fim == default)
        {
            throw new ValidacaoException(
                codigo: "periodo_obrigatorio",
                mensagem: "O início e o fim do período são obrigatórios."
            );
        }

        if (command.Fim <= command.Inicio)
        {
            throw new ValidacaoException(
                codigo: "periodo_invalido",
                mensagem: "O fim do período deve ser posterior ao início."
            );
        }

        var motivo = string.IsNullOrWhiteSpace(command.Motivo)
            ? MotivoPadrao
            : command.Motivo.Trim();
        if (motivo.Length > TamanhoMaximoMotivo)
        {
            throw new ValidacaoException(
                codigo: "motivo_muito_longo",
                mensagem: $"O motivo da indisponibilidade deve ter no máximo {TamanhoMaximoMotivo} caracteres."
            );
        }

        if (!command.SeRepete)
        {
            return (motivo, null, null);
        }

        if (string.IsNullOrWhiteSpace(command.Frequencia))
        {
            throw new ValidacaoException(
                codigo: "frequencia_obrigatoria",
                mensagem: "A frequência de repetição é obrigatória quando a indisponibilidade se repete."
            );
        }

        var frequenciaEncontrada = FrequenciasPermitidas.FirstOrDefault(
            f => string.Equals(f, command.Frequencia.Trim(), StringComparison.OrdinalIgnoreCase)
        ) ?? throw new ValidacaoException(
            codigo: "frequencia_invalida",
            mensagem: "A frequência informada é inválida. Valores aceitos: Daily, Weekly, Monthly ou Yearly."
        );

        var limiteMaximoRepeticao = command.Inicio.AddYears(1);
        var repetirAte = command.RepetirAte ?? limiteMaximoRepeticao;
        if (repetirAte <= command.Fim)
        {
            throw new ValidacaoException(
                codigo: "repetir_ate_invalido",
                mensagem: "A data limite de repetição deve ser posterior ao fim do evento inicial."
            );
        }

        if (repetirAte > limiteMaximoRepeticao)
        {
            throw new ValidacaoException(
                codigo: "repetir_ate_excede_limite",
                mensagem: "A repetição da indisponibilidade não pode ultrapassar 1 ano a partir da data de início."
            );
        }
        return (motivo, frequenciaEncontrada, repetirAte);
    }
}