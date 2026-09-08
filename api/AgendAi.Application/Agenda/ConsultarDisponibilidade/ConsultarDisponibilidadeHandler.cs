namespace AgendAi.Application.Agenda.ConsultarDisponibilidade;

using AgendAi.Application.Agenda.Models;
using AgendAi.Application.Agenda.Ports;
using AgendAi.Application.Common.Exceptions;

public sealed class ConsultarDisponibilidadeHandler
{
    private const int DiasBuscaPadrao = 7;
    private const int LimiteMinimo = 1;
    private const int LimiteMaximo = 3;
    private const string TimezonePadrao = "America/Sao_Paulo";

    private static readonly TimeZoneInfo TimezoneApp = TimeZoneInfo.FindSystemTimeZoneById(TimezonePadrao);

    private readonly IDisponibilidadeReader _disponibilidadeReader;
    private readonly IDisponibilidadeExternaGateway _disponibilidadeExternaGateway;
    
    public ConsultarDisponibilidadeHandler(
        IDisponibilidadeReader disponibilidadeReader,
        IDisponibilidadeExternaGateway disponibilidadeExternaGateway
    )
    {
        _disponibilidadeReader = disponibilidadeReader;
        _disponibilidadeExternaGateway = disponibilidadeExternaGateway;
    }

    public async Task<ConsultarDisponibilidadeResult> HandleAsync(
        ConsultarDisponibilidadeQuery query,
        CancellationToken cancellationToken
    )
    {
        ArgumentNullException.ThrowIfNull(query);

        ValidarQuery(query);

        var dados = await _disponibilidadeReader.ObterAsync(
            query.ProfissionalUuid,
            query.ProfissionalProcedimentoUuid,
            cancellationToken
        ) ?? throw new RecursoNaoEncontradoException(
            codigo: "profissional_procedimento_nao_encontrado",
            mensagem: "O procedimento informado não está disponivel para este profissional."
        );

        ValidarConfigAtendimento(dados);

        var fimSolicitado = query.Inicio.AddMinutes(dados.DuracaoMinutos);

        ValidarPeriodoSolicitado(
            query.Inicio,
            fimSolicitado,
            dados.JanelasAtendimento
        );

        var idAgendaExterna = ObterIdAgendaExterna(dados);
        var webhookUri = ObterWebhookUri(dados);

        var consultarExterna = new ConsultaDisponibilidadeExterna(
            ProfissionalUuid: dados.ProfissionalUuid,
            IdAgendaExterna: idAgendaExterna,
            Timezone: TimezonePadrao,
            Inicio: query.Inicio,
            Fim: fimSolicitado,
            QuantidadeAlternativas: query.Limite,
            DiasBusca: DiasBuscaPadrao,
            DuracaoMinutos: dados.DuracaoMinutos,
            JanelasAtendimento: dados.JanelasAtendimento
        );

        var resultExterno = await _disponibilidadeExternaGateway.ConsultarAsync(
            consultarExterna,
            webhookUri,
            cancellationToken
        );

        ValidarHorariosRetornados(resultExterno, dados.JanelasAtendimento);

        var horariosDisponiveis = resultExterno
            .HorariosDisponiveis
            .Select(horario => new HorarioDisponivelResult(
                Inicio: horario.Inicio,
                Fim: horario.Fim
            ))
        .ToArray();

        return new ConsultarDisponibilidadeResult(
            Disponivel: resultExterno.Disponivel,
            DuracaoMinutos: dados.DuracaoMinutos,
            HorariosDisponiveis: horariosDisponiveis,
            BuscaEsgotada: resultExterno.BuscaEsgotada
        );
    }

    private static void ValidarQuery(ConsultarDisponibilidadeQuery query)
    {
        if (query.ProfissionalUuid == Guid.Empty)
        {
            throw new ValidacaoException(
                codigo: "profissional_uuid_invalido",
                mensagem: "O UUID do profissional é obrigatório."
            );
        }

        if (query.ProfissionalProcedimentoUuid == Guid.Empty)
        {
            throw new ValidacaoException(
                codigo: "profissional_procedimento_uuid_invalido",
                mensagem: "O UUID do procedimento é obrigatório."
            );
        }

        if (query.Inicio == default)
        {
            throw new ValidacaoException(
                codigo: "inicio_obrigatorio",
                mensagem: "O inicio solicitado é obrigatório."
            );
        }

        if (query.Limite < LimiteMinimo || query.Limite > LimiteMaximo)
        {
            throw new ValidacaoException(
                codigo: "limite_invalido",
                mensagem: $"O limite de alternativas deve estar entre {LimiteMinimo} e {LimiteMaximo}."
            );
        }
    }

    private static void ValidarConfigAtendimento(DadosDisponibilidadeProfissional dados)
    {
        if (dados.DuracaoMinutos <= 0)
        {
            throw new ConflitoException(
                codigo: "duracao_procedimento_invalida",
                mensagem: "O procedimento não possui uma duração válida configurada."
            );
        }

        if (dados.JanelasAtendimento.Count == 0)
        {
            throw new ConflitoException(
                codigo: "horarios_atendimento_nao_configurados",
                mensagem: "A clínica ainda não possui horários de atendimento configurados."
            );
        }
    }

    private static bool PeriodoDentroDeJanela(
        DateTimeOffset inicio,
        DateTimeOffset fim,
        IReadOnlyCollection<DadosJanelaAtendimento> janelasAtendimento
    )
    {
        var inicioLocal = TimeZoneInfo.ConvertTime(inicio, TimezoneApp);
        var fimLocal = TimeZoneInfo.ConvertTime(fim, TimezoneApp);

        if (inicioLocal.Date != fimLocal.Date)
        {
            return false;
        }

        var diaSemana = (short)inicioLocal.DayOfWeek;

        return janelasAtendimento.Any(janela => 
            janela.DiaSemana == diaSemana
            && inicioLocal.TimeOfDay >= janela.HoraInicio
            && fimLocal.TimeOfDay <= janela.HoraFim
        );
    }

    private static void ValidarPeriodoSolicitado(
        DateTimeOffset inicio,
        DateTimeOffset fim,
        IReadOnlyCollection<DadosJanelaAtendimento> janelasAtendimento
    )
    {
        if (!PeriodoDentroDeJanela(inicio, fim, janelasAtendimento))
        {
            throw new ValidacaoException(
                codigo: "horario_fora_do_atendimento",
                mensagem: "O horário solicitado está fora do período de atendimento da clínica."
            );
        }
    }
    
    private static string ObterIdAgendaExterna(DadosDisponibilidadeProfissional dados)
    {
        if (string.IsNullOrWhiteSpace(dados.IdAgendaExterna))
        {
            throw new ConflitoException(
                codigo: "agenda_nao_configurada",
                mensagem: "O profissional não possui uma agenda configurada."
            );
        }

        return dados.IdAgendaExterna;
    }

    private static Uri ObterWebhookUri(DadosDisponibilidadeProfissional dados)
    {
        if (string.IsNullOrWhiteSpace(dados.UrlWebhookAgenda))
        {
            throw new ConflitoException(
                codigo: "webhook_nao_configurado",
                mensagem: "A clínica não possui um webhook de agenda configurado."
            );
        }

        if (!Uri.TryCreate(
            dados.UrlWebhookAgenda,
            UriKind.Absolute,
            out var webhookUri
        )
        || 
        (
            webhookUri.Scheme != Uri.UriSchemeHttp && webhookUri.Scheme != Uri.UriSchemeHttps
        ))
        {
            throw new ConflitoException(
                codigo: "webhook_invalido",
                mensagem: "O webhook de agenda configurado nessa clínica é inválido."
            );
        }

        return webhookUri;
    }

    private static void ValidarHorariosRetornados(
        ResultadoDisponibilidadeExterna result,
        IReadOnlyCollection<DadosJanelaAtendimento> janelasAtendimento
    )
    {
        var isHorarioInvalido = result
            .HorariosDisponiveis
            .Any(horario => !PeriodoDentroDeJanela(
                horario.Inicio,
                horario.Fim,
                janelasAtendimento
            ));

        if(isHorarioInvalido)
        {
            throw new IntegracaoExternaException(
                codigo: "n8n_disponibilidade_fora_de_atendimento",
                mensagem: "O n8n retornou um horário fora do periodo de atendimento."
            );
        }
    }

}