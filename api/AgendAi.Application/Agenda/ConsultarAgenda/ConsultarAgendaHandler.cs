namespace AgendAi.Application.Agenda.ConsultarAgenda;
using AgendAi.Application.Agenda.Ports;
using AgendAi.Application.Common.Exceptions;

public sealed class ConsultarAgendaHandler
{
    private const int MaximoDiasConsulta = 45;
    private readonly IProfissionalAgendaReader _profissionalAgendaReader;
    private readonly IAgendaExternaGateway _agendaExternaGateway;
    private readonly IAgendamentoAgendaReader _agendamentoAgendaReader;

    public ConsultarAgendaHandler (
        IProfissionalAgendaReader profissionalAgendaReader,
        IAgendaExternaGateway agendaExternaGateway,
        IAgendamentoAgendaReader agendamentoAgendaReader
    )
    {
        _profissionalAgendaReader = profissionalAgendaReader;
        _agendaExternaGateway = agendaExternaGateway;
        _agendamentoAgendaReader = agendamentoAgendaReader;
    }

    public async Task<ConsultarAgendaResult> HandleAsync(
        ConsultarAgendaQuery query,
        CancellationToken cancellationToken
    )
    {
        ArgumentNullException.ThrowIfNull(query);
        ValidarQuery(query);

        var dadosProfissional = 
            await _profissionalAgendaReader.ObterPorUuidAsync(query.ProfissionalUuid, cancellationToken) 
            ?? throw new RecursoNaoEncontradoException(
                codigo: "profissional_nao_encontrado",
                mensagem: "O profissional informado não foi encontrado"
            );

        var idAgendaExterna = dadosProfissional.IdAgendaExterna;
        if(string.IsNullOrWhiteSpace(idAgendaExterna))
        {
            throw new ConflitoException(
                codigo: "agenda_nao_configurada",
                mensagem: "O profissional ainda não possui uma agenda configurada."
            );
        }

        var urlWebhookAgenda = dadosProfissional.UrlWebhookAgenda;
        if(string.IsNullOrWhiteSpace(urlWebhookAgenda))
        {
            throw new ConflitoException(
                codigo: "webhook_nao_configurado",
                mensagem: "A clinica ainda não possui um webhook de agenda configurado."
            );
        }

        if(
            !Uri.TryCreate(
                urlWebhookAgenda,
                UriKind.Absolute,
                out var webhookUri
            )
            || 
            (webhookUri.Scheme != Uri.UriSchemeHttp && webhookUri.Scheme != Uri.UriSchemeHttps)
        )
        {
            throw new ConflitoException(
                codigo: "webhook_invalido",
                mensagem: "O webhook de agenda configurado para a clinica é inválido."
            );
        }

        var eventosExternos = await _agendaExternaGateway.ConsultarAsync(
            profissionalUuid: query.ProfissionalUuid,
            idAgendaExterna: idAgendaExterna,
            urlWebhookAgenda: webhookUri,
            inicio: query.Inicio,
            fim: query.Fim,
            cancellationToken: cancellationToken
        );

        var idsEventosExternos = eventosExternos
            .Where(evento => evento.Tipo == "agendamento")
            .Select(evento => evento.IdExterno)
            .Distinct()
            .ToArray();

        var dadosAgendamentos = await _agendamentoAgendaReader.ListarPorIdsEventosExternosAsync(
            query.ProfissionalUuid,
            idsEventosExternos,
            cancellationToken
        );

        var agendamentoUuidPorEvento = dadosAgendamentos.ToDictionary(
            dado => dado.IdEventoExterno,
            dado => dado.AgendamentoUuid
        );

        var eventos = eventosExternos.Select(evento => new EventoAgendaResult(
            Id: evento.IdExterno,
            Titulo: evento.Titulo,
            Tipo: evento.Tipo,
            AgendamentoUuid: evento.Tipo == "agendamento" && agendamentoUuidPorEvento.TryGetValue(
                evento.IdExterno,
                out var uuidEncontrado
            ) ? uuidEncontrado : null,
            Inicio: evento.Inicio,
            Fim: evento.Fim,
            NomeCliente: evento.NomeCliente,
            NomeProcedimento: evento.NomeProcedimento,
            ProfissionalUuid: evento.ProfissionalUuid
        )).ToArray();

        return new ConsultarAgendaResult(eventos);
    }

    private static void ValidarQuery(ConsultarAgendaQuery query)
    {
        if(query.ProfissionalUuid == Guid.Empty)
        {
            throw new ValidacaoException(
                codigo: "profissional_uuid_invalido",
                mensagem: "O UUID do profissional é obrigatório."
            );
        }

        if(query.Inicio == default || query.Fim == default)
        {
            throw new ValidacaoException(
                codigo: "periodo_obrigatorio",
                mensagem: "O inicio e fim do periodo são obrigatórios."
            );
        }

        if(query.Fim <= query.Inicio)
        {
            throw new ValidacaoException(
                codigo: "periodo_invalido",
                mensagem: "O fim do periodo deve ser posterior ao inicio."
            );
        }

        if(query.Fim - query.Inicio > TimeSpan.FromDays(MaximoDiasConsulta))
        {
            throw new ValidacaoException(
                codigo: "periodo_muito_longo",
                mensagem: $"O periodo não pode passar de {MaximoDiasConsulta} dias."
            );
        }
    }

}
