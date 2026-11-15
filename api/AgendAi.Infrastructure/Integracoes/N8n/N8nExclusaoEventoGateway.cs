namespace AgendAi.Infrastructure.Integracoes.N8n;

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AgendAi.Application.Agenda.Models;
using AgendAi.Application.Agenda.Ports;
using AgendAi.Application.Common.Exceptions;

public sealed class N8nExclusaoEventoGateway : IExclusaoEventoAgendaExternaGateway
{
    private const int QtdeMaximaRetries = 2;
    private readonly HttpClient _httpClient;

    public N8nExclusaoEventoGateway( HttpClient httpClient )
    {
        _httpClient = httpClient;
    }

    public async Task<ResultadoExclusaoEventoAgendaExterna> ExcluirAsync(
        ExclusaoEventoAgendaExterna exclusao,
        Uri urlWebhookAgenda,
        CancellationToken cancellationToken
    )
    {
        ArgumentNullException.ThrowIfNull(exclusao);
        ArgumentNullException.ThrowIfNull(urlWebhookAgenda);

        var req = CriarRequest(exclusao);

        for (var i = 1; i < QtdeMaximaRetries; i++)
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
                        "n8n_exclusao_evento_erro",
                        $"O n8n respondeu com {(int)resHttp.StatusCode}."
                    );
                }

                var resN8n = await resHttp.Content
                    .ReadFromJsonAsync<ExcluirEventoN8nResponse>(cancellationToken)
                    ?? throw new IntegracaoExternaException(
                        "n8n_exclusao_evento_resposta_vazia",
                        "O n8n retornou uma resposta vazia."
                    );

                return MapearResposta(resN8n, exclusao);
            }
            catch(OperationCanceledException exception)
            when (!cancellationToken.IsCancellationRequested)
            {
                if (CanRetry(i))
                {
                    continue;
                }
                throw new IntegracaoExternaException(
                    codigo: "n8n_exclusao_evento_timeout",
                    mensagem: "O n8n demorou mais que o previsto para excluir o evento.",
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
                    codigo: "n8n_exclusao_evento_indisponivel",
                    mensagem: "Não foi possível se conectar ao n8n.",
                    innerException: exception
                );
            }

            catch (JsonException exception)
            {
                throw new IntegracaoExternaException(
                    codigo: "n8n_exclusao_evento_resposta_invalida",
                    mensagem: "O n8n retornou um JSON inválido.",
                    innerException: exception
                );
            }
        }
        
        throw new IntegracaoExternaException(
            codigo: "n8n_exclusao_evento_falha",
            mensagem: "Não foi possível excluir o evento externo."
        );
    }

    private static ExcluirEventoN8nRequest CriarRequest(ExclusaoEventoAgendaExterna exclusao)
    {
        return new ExcluirEventoN8nRequest(
            Operacao: "excluir_evento",
            Evento: new EventoExclusaoN8nRequest(
                IdEventoCalendar: exclusao.IdEventoExterno
            ),
            Profissional: new ProfissionalExclusaoEventoN8nRequest(
                IdCalendar: exclusao.IdAgendaExterna
            )
        );
    }

    private static ResultadoExclusaoEventoAgendaExterna MapearResposta(
        ExcluirEventoN8nResponse response,
        ExclusaoEventoAgendaExterna exclusao
    )
    {
        if (!response.Sucesso)
        {
            throw new IntegracaoExternaException(
                codigo: "n8n_exclusao_evento_rejeitada",
                mensagem: "O n8n não conseguiu excluir o evento."
            );
        }

        if (string.IsNullOrWhiteSpace(response.IdEvent))
        {
            throw new IntegracaoExternaException(
                codigo: "n8n_exclusao_evento_id_evento_ausente",
                mensagem: "O n8n não retornou o identificador do evento excluído."
            );
        }

        return new ResultadoExclusaoEventoAgendaExterna(
            IdEventoExterno: response.IdEvent
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
