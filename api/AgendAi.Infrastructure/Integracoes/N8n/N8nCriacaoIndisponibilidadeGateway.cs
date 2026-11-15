namespace AgendAi.Infrastructure.Integracoes.N8n;

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AgendAi.Application.Agenda.Models;
using AgendAi.Application.Agenda.Ports;
using AgendAi.Application.Common.Exceptions;

public sealed class N8nCriacaoIndisponibilidadeGateway : ICriacaoIndisponibilidadeExternaGateway
{
    private const int QtdeMaximaRetries = 2;
    private readonly HttpClient _httpClient;

    public N8nCriacaoIndisponibilidadeGateway(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ResultadoCriacaoIndisponibilidadeExterna> CriarAsync(
        CriacaoIndisponibilidadeExterna criacao,
        Uri urlWebhookAgenda,
        CancellationToken cancellationToken
    )
    {
        ArgumentNullException.ThrowIfNull(criacao);
        ArgumentNullException.ThrowIfNull(urlWebhookAgenda);

        var req = CriarRequest(criacao);

        for (var i = 1; i <= QtdeMaximaRetries; i++)
        {
            try
            {
                using var resHttp = await _httpClient.PostAsJsonAsync(
                    urlWebhookAgenda,
                    req,
                    cancellationToken
                );

                if (!resHttp.IsSuccessStatusCode)
                {
                    if (IsStatusTransitorio(resHttp.StatusCode) && CanRetry(i))
                    {
                        continue;
                    }

                    throw new IntegracaoExternaException(
                        codigo: "n8n_criacao_indisponibilidade_erro_http",
                        mensagem: $"O n8n respondeu com {(int)resHttp.StatusCode}."
                    );
                }

                var resN8n = await resHttp.Content
                    .ReadFromJsonAsync<CriarIndisponibilidadeN8nResponse>(cancellationToken)
                    ?? throw new IntegracaoExternaException(
                        codigo: "n8n_criacao_indisponibilidade_resposta_vazia",
                        mensagem: "O n8n retornou uma resposta vazia."
                    );

                return MapearResposta(resN8n, criacao);
            }
            catch (OperationCanceledException exception)
            when (!cancellationToken.IsCancellationRequested)
            {
                if (CanRetry(i))
                {
                    continue;
                }

                throw new IntegracaoExternaException(
                    codigo: "n8n_criacao_indisponibilidade_timeout",
                    mensagem: "O n8n demorou mais que o previsto para criar a indisponibilidade.",
                    innerException: exception
                );
            }

            catch (HttpRequestException exception)
            {
                if (CanRetry(i))
                {
                    continue;
                }

                throw new IntegracaoExternaException(
                    codigo: "n8n_criacao_indisponibilidade_indisponivel",
                    mensagem: "Não foi possível se conectar ao n8n.",
                    innerException: exception
                );
            }
            
            catch (JsonException exception)
            {
                throw new IntegracaoExternaException(
                    codigo: "n8n_criacao_indisponibilidade_resposta_invalida",
                    mensagem: "O n8n retornou um JSON inválido.",
                    innerException: exception
                );
            }
        }

        throw new IntegracaoExternaException(
            codigo: "n8n_criacao_indisponibilidade_falha",
            mensagem: "Não foi possível criar a indisponibilidade externa."
        );
    }

    private static CriarIndisponibilidadeN8nRequest CriarRequest(
        CriacaoIndisponibilidadeExterna criacao
    )
    {
        return new CriarIndisponibilidadeN8nRequest(
            Operacao: "criar_indisponibilidade",
            Evento: new EventoCriacaoIndisponibilidadeN8nRequest(
                HoraInicio: criacao.Inicio,
                HoraFim: criacao.Fim,
                SeRepete: criacao.SeRepete,
                Frequencia: criacao.Frequencia,
                RepetirAte: criacao.RepetirAte
            ),
            Profissional: new ProfissionalCriacaoIndisponibilidadeN8nRequest(
                IdCalendar: criacao.IdAgendaExterna
            ),
            Motivo: criacao.Motivo
        );
    }

    private static ResultadoCriacaoIndisponibilidadeExterna MapearResposta(
        CriarIndisponibilidadeN8nResponse response,
        CriacaoIndisponibilidadeExterna criacao
    )
    {
        if (!response.Sucesso)
        {
            throw new IntegracaoExternaException(
                codigo: "n8n_criacao_indisponibilidade_rejeitada",
                mensagem: "O n8n não conseguiu criar a indisponibilidade."
            );
        }

        if (string.IsNullOrWhiteSpace(response.IdEvent))
        {
            throw new IntegracaoExternaException(
                codigo: "n8n_criacao_indisponibilidade_id_evento_ausente",
                mensagem: "O n8n não retornou o identificador da indisponibilidade criada."
            );
        }

        var horarioSolicitado = response.HorarioSolicitado
            ?? throw new IntegracaoExternaException(
                codigo: "n8n_criacao_indisponibilidade_periodo_ausente",
                mensagem: "O n8n não retornou o período da indisponibilidade."
            );

        if (horarioSolicitado.Inicio != criacao.Inicio || horarioSolicitado.Fim != criacao.Fim)
        {
            throw new IntegracaoExternaException(
                codigo: "n8n_criacao_indisponibilidade_periodo_divergente",
                mensagem: "O n8n retornou um período diferente do solicitado."
            );
        }

        return new ResultadoCriacaoIndisponibilidadeExterna(
            IdEventoExterno: response.IdEvent,
            Inicio: horarioSolicitado.Inicio,
            Fim: horarioSolicitado.Fim,
            Frequencia: response.Frequencia,
            RepetirAte: response.RepeteAte
        );
    }

    private static bool CanRetry(int tentativaAtual)
    {
        return tentativaAtual < QtdeMaximaRetries;
    }

    private static bool IsStatusTransitorio(HttpStatusCode statusCode)
    {
        return statusCode == HttpStatusCode.RequestTimeout
            || statusCode == HttpStatusCode.TooManyRequests
            || (int)statusCode >= 500;
    }
}