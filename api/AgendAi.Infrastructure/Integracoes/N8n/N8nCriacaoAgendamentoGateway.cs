namespace AgendAi.Infrastructure.Integracoes.N8n;

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AgendAi.Application.Agenda.Models;
using AgendAi.Application.Agenda.Ports;
using AgendAi.Application.Common.Exceptions;

public sealed class N8nCriacaoAgendamentoGateway : ICriacaoAgendamentoExternoGateway
{
    private const int QtdeMaximaRetries = 2;
    private readonly HttpClient _httpClient;

    public N8nCriacaoAgendamentoGateway( HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ResultadoCriacaoAgendamentoExterno> CriarAsync(
        CriacaoAgendamentoExterno create,
        Uri urlWebhookAgenda,
        CancellationToken cancellationToken
    )
    {
        ArgumentNullException.ThrowIfNull(create);
        ArgumentNullException.ThrowIfNull(urlWebhookAgenda);

        var idEventoEsperado = CriarIdEventoExterno(create.AgendamentoUuid);

        var req = CriarRequest(
            create,
            idEventoEsperado
        );

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
                        codigo: "n8n_criacao_agendamento_erro_http",
                        mensagem: $"O n8n respondeu com {(int)resHttp.StatusCode}."
                    );
                }

                var resN8n = await resHttp.Content
                    .ReadFromJsonAsync<CriarAgendamentoN8nResponse>(cancellationToken) 
                    ?? throw new IntegracaoExternaException(
                        codigo: "n8n_criacao_agendamento_resposta_vazia",
                        mensagem: "O n8n retornou uma resposta vazia."
                    );

                return MapearResposta(
                    resN8n,
                    create,
                    idEventoEsperado
                );
            }
            catch (OperationCanceledException exception)
            when(!cancellationToken.IsCancellationRequested)
            {
                if (CanRetry(i))
                {
                    continue;
                }

                throw new IntegracaoExternaException(
                    codigo: "n8n_criacao_agendamento_timeout",
                    mensagem: "O n8n demorou mais que o previsto para criar o agendamento.",
                    innerException: exception 
                );
            }
            
            catch (HttpRequestException exception)
            {
                if(CanRetry(i))
                {
                    continue;
                }

                throw new IntegracaoExternaException(
                    codigo: "n8n_criacao_agendamento_indisponivel",
                    mensagem: "Não foi possivel se conectar ao n8n.",
                    innerException: exception
                );
            }

            catch (JsonException exception)
            {
                throw new IntegracaoExternaException(
                    codigo: "n8n_criacao_agendamento_resposta_invalida",
                    mensagem: "O n8n retornou um json invalido.",
                    innerException: exception
                );
            }
        }

        throw new IntegracaoExternaException(
            codigo: "n8n_criacao_agendamento_falha",
            mensagem: "Não foi possível criar o evento externo."
        );
    }

    private static CriarAgendamentoN8nRequest CriarRequest (
        CriacaoAgendamentoExterno create,
        string idEventoEsperado
    )
    {
        return new CriarAgendamentoN8nRequest(
            Operacao: "criar_agendamento",

            Evento: new EventoCriacaoN8nRequest(
                Uuid: create.AgendamentoUuid,
                IdEventoCalendar: idEventoEsperado,
                HoraInicio: create.Inicio,
                HoraFim: create.Fim,
                MotivoContato: create.MotivoContato
            ),

            Profissional: new ProfissionalCriacaoAgendamentoN8nRequest(
                Uuid: create.ProfissionalUuid,
                IdCalendar: create.IdAgendaExterna,
                Procedimento: create.NomeProcedimento
            ),

            Cliente: new ClienteCricaoAgendamentoN8nRequest(
                Nome: create.NomeCliente
            )
        );
    }

    private static ResultadoCriacaoAgendamentoExterno MapearResposta(
        CriarAgendamentoN8nResponse response,
        CriacaoAgendamentoExterno create,
        string idEventoEsperado
    )
    {
        if (!response.Sucesso)
        {
            throw new IntegracaoExternaException(
                codigo: "n8n_criacao_agendamento_rejeitada",
                mensagem: "O n8n não conseguiu criar o agendamento."
            );
        }

        if (string.IsNullOrWhiteSpace(response.IdEvent))
        {
            throw new IntegracaoExternaException(
                codigo: "n8n_criacao_agendamento_id_evento_ausente",
                mensagem: "O n8n não retornou o identificador do evento criado."
            );
        }

        if (!string.Equals(response.IdEvent, idEventoEsperado, StringComparison.Ordinal))
        {
            throw new IntegracaoExternaException(
                codigo: "n8n_criacao_agendamento_id_evento_divergente",
                mensagem: "O n8n retornou um id de evento diferente do solicitado."
            );
        }

        if (!response.AgendamentoUuid.HasValue)
        {
            throw new IntegracaoExternaException(
                codigo: "n8n_criacao_agendamento_uuid_ausente",
                mensagem: "O n8n não retornou o UUID do agendamento."
            );
        }

        if (response.AgendamentoUuid.Value != create.AgendamentoUuid)
        {
            throw new IntegracaoExternaException(
                codigo: "n8n_criacao_agendamento_uuid_divergente",
                mensagem: "O n8n retornou o UUID de outro agendamento."
            );
        }

        var horarioSolicitado = response.HorarioSolicitado 
            ?? throw new IntegracaoExternaException(
                codigo: "n8n_criacao_agendamento_periodo_ausente",
                mensagem: "O n8n não retornou o período do agendamento."
            );

        if (horarioSolicitado.Inicio != create.Inicio || horarioSolicitado.Fim != create.Fim)
        {
            throw new IntegracaoExternaException(
                codigo: "n8n_criacao_agendamento_periodo_divergente",
                mensagem: "O n8n retornou um período diferente do solicitado."
            );
        }

        return new ResultadoCriacaoAgendamentoExterno(
            IdEventoExterno: response.IdEvent,
            AgendamentoUuid: response.AgendamentoUuid.Value,
            Inicio: horarioSolicitado.Inicio,
            Fim: horarioSolicitado.Fim
        );
    }

    private static string CriarIdEventoExterno(Guid agendamentoUuid)
    {
        return $"ag{agendamentoUuid:N}";
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