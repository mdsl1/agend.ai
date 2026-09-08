namespace AgendAi.Infrastructure.Integracoes.N8n;
using System.Net.Http.Json;
using System.Text.Json;
using AgendAi.Application.Agenda.Models;
using AgendAi.Application.Agenda.Ports;
using AgendAi.Application.Common.Exceptions;

public sealed class N8nDisponibilidadeGateway : IDisponibilidadeExternaGateway
{
    private readonly HttpClient _httpClient;
    public N8nDisponibilidadeGateway (HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ResultadoDisponibilidadeExterna> ConsultarAsync (
        ConsultaDisponibilidadeExterna consulta,
        Uri urlWebhookAgenda,
        CancellationToken cancellationToken
    )
    {
        var req = CriarRequest(consulta);

        try
        {
            using var resHttp = await _httpClient.PostAsJsonAsync(
                urlWebhookAgenda,
                req,
                cancellationToken
            );

            if(!resHttp.IsSuccessStatusCode)
            {
                throw new IntegracaoExternaException(
                    codigo: "n8n_disponibilidade_erro_http",
                    mensagem: $"O n8n respondeu com HTTP {(int)resHttp.StatusCode}."
                );
            }

            var resN8n = await resHttp.Content
                .ReadFromJsonAsync<ConsultarDisponibilidadeN8nResponse> ( cancellationToken )
                ?? throw new IntegracaoExternaException(
                    codigo: "n8n_disponibilidade_resposta_vazia",
                    mensagem: "O n8n retornou uma resposta vazia."
                );

            if(!resN8n.Sucesso)
            {
                throw new IntegracaoExternaException(
                    codigo: "n8n_disponibilidade_rejeitada",
                    mensagem: "O n8n não conseguiu consultar a disponibilidade."
                );
            }

            return MapearResposta(resN8n, consulta);
        }
        catch (IntegracaoExternaException)
        {
            throw;
        }
        catch (OperationCanceledException exception) when (!cancellationToken.IsCancellationRequested)
        {
            throw new IntegracaoExternaException(
                codigo: "n8n_disponibilidade_timeout",
                mensagem: "O n8n demorou mais que o previsto para responder.",
                innerException: exception
            );
        }
        catch (HttpRequestException exception)
        {
            throw new IntegracaoExternaException(
                codigo: "n8n_disponibilidade_indisponivel",
                mensagem: "Não foi possível se conectar ao n8n.",
                innerException: exception
            );
        }
        catch (JsonException exception)
        {
            throw new IntegracaoExternaException(
                codigo: "n8n_disponibilidade_resposta_invalida",
                mensagem: "O n8n retornou um JSON inválido.",
                innerException: exception
            );
        }
    }

    private static ConsultarDisponibilidadeN8nRequest CriarRequest(
        ConsultaDisponibilidadeExterna consulta
    )
    {
        var janelasAtendimento = consulta.JanelasAtendimento
            .Select(janela => new JanelaAtendimentoN8nRequest(
                DiaSemana: janela.DiaSemana,
                Inicio: CombinarDataComHorario(
                    consulta.Inicio,
                    janela.HoraInicio
                ),
                Fim: CombinarDataComHorario(
                    consulta.Inicio,
                    janela.HoraFim
                )
            )
        ).ToArray();

        return new ConsultarDisponibilidadeN8nRequest (
            Operacao: "consultar_disponibilidade",
            Timezone: consulta.Timezone,
            Profissional: new ProfissionalN8nRequest(
                Uuid: consulta.ProfissionalUuid,
                IdCalendar: consulta.IdAgendaExterna
            ),
            PeriodoSolicitado: new PeriodoN8nRequest(
                Inicio: consulta.Inicio,
                Fim: consulta.Fim
            ),
            JanelasAtendimento: janelasAtendimento,
            Opcoes: new OpcoesDisponibilidadeN8nRequest(
                QuantidadeAlternativas: consulta.QuantidadeAlternativas,
                DiasBusca: consulta.DiasBusca,
                DuracaoSessaoMinutos: consulta.DuracaoMinutos
            )
        );
    }

    private static DateTimeOffset CombinarDataComHorario(
        DateTimeOffset dataRef,
        TimeSpan horario
    )
    {
        return new DateTimeOffset(
            year: dataRef.Year,
            month: dataRef.Month,
            day: dataRef.Day,
            hour: horario.Hours,
            minute: horario.Minutes,
            second: horario.Seconds,
            offset: dataRef.Offset 
        );
    }

    private static ResultadoDisponibilidadeExterna MapearResposta(
        ConsultarDisponibilidadeN8nResponse res,
        ConsultaDisponibilidadeExterna consulta
    )
    {
        if (res.ProfissionalUuid != consulta.ProfissionalUuid)
        {
            throw new IntegracaoExternaException(
                codigo: "n8n_disponibilidade_profissional_invalido",
                mensagem: "O n8n retornou dados de outro profissional."
            );
        }

        var periodoSolicitado = res.PeriodoSolicitado
            ?? throw new IntegracaoExternaException(
                codigo: "n8n_disponibilidade_periodo_ausente",
                mensagem: "O n8n não retornou o período solicitado."
            );
        
        if (periodoSolicitado.Inicio != consulta.Inicio || periodoSolicitado.Fim != consulta.Fim)
        {
            throw new IntegracaoExternaException(
                codigo: "n8n_disponibilidade_periodo_divergente",
                mensagem: "O n8n retornou um período diferente do solicitado."
            );
        }

        if (res.Disponivel)
        {
            var horarioSolicitado = new HorarioDisponivelExterno(
                Inicio: periodoSolicitado.Inicio,
                Fim: periodoSolicitado.Fim
            );

            return new ResultadoDisponibilidadeExterna(
                Disponivel: true,
                HorariosDisponiveis: new[] {horarioSolicitado},
                BuscaEsgotada: false
            );
        }

        var alternativas = MapearAlternativas(
            res.AlternativasDisponiveis,
            consulta
        );

        return new ResultadoDisponibilidadeExterna(
            Disponivel: false,
            HorariosDisponiveis: alternativas,
            BuscaEsgotada: res.BuscaEsgotada
        );
    }

    private static IReadOnlyCollection<HorarioDisponivelExterno> MapearAlternativas(
        IReadOnlyCollection<HorarioDisponivelN8nResponse>? alternativas,
        ConsultaDisponibilidadeExterna consulta
    )
    {
        var duracaoEstimada = TimeSpan.FromMinutes(consulta.DuracaoMinutos);

        var inicioBusca = consulta.Fim;
        var fimBusca = consulta.Fim.AddDays(consulta.DiasBusca);

        return (alternativas ?? Array.Empty<HorarioDisponivelN8nResponse>())
            .Select(alternativa =>
            {
                if (alternativa.Fim <= alternativa.Inicio)
                {
                    throw new IntegracaoExternaException(
                        codigo: "n8n_disponibilidade_alternativa_invalida",
                        mensagem: "O n8n retornou uma alternativa com período inválido."
                    );
                }

                if (alternativa.Fim - alternativa.Inicio != duracaoEstimada)
                {
                    throw new IntegracaoExternaException(
                        codigo: "n8n_disponibilidade_duracao_invalida",
                        mensagem:
                            "O n8n retornou uma alternativa com duração inválida."
                    );
                }

                if (alternativa.Inicio < inicioBusca || alternativa.Fim >fimBusca)
                {
                    throw new IntegracaoExternaException(
                        codigo: "n8n_disponibilidade_alternativa_fora_da_busca",
                        mensagem: "O n8n retornou uma alternativa fora do período de busca."
                    );
                }

                return new HorarioDisponivelExterno(
                    Inicio: alternativa.Inicio,
                    Fim: alternativa.Fim
                );
            })
            .DistinctBy(horario => new
            {
                horario.Inicio,
                horario.Fim
            })
            .OrderBy(horario => horario.Inicio)
            .Take(consulta.QuantidadeAlternativas)
        .ToArray();
    }

}