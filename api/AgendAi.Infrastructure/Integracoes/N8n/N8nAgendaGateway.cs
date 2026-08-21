namespace AgendAi.Infrastructure.Integracoes.N8n;
using System.Net.Http.Json;
using System.Text.Json;
using AgendAi.Application.Agenda.Models;
using AgendAi.Application.Agenda.Ports;
using AgendAi.Application.Common.Exceptions;

public sealed class N8nAgendaGateway : IAgendaExternaGateway
{
    private readonly HttpClient _httpClient;

    public N8nAgendaGateway(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyCollection<EventoAgendaExterna>> ConsultarAsync (
        Guid profissionalUuid,
        string idAgendaExterna,
        Uri urlWebhookAgenda,
        DateTimeOffset inicio,
        DateTimeOffset fim,
        CancellationToken cancellationToken
    )
    {
        var req = new ConsultarAgendaN8nRequest(
            Operacao: "consultar_agenda",
            Profissional: new ProfissionalN8nRequest(
                Uuid: profissionalUuid,
                IdCalendar: idAgendaExterna
            ),
            Periodo: new PeriodoN8nRequest(
                Inicio: inicio,
                Fim: fim
            )
        );

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
                    codigo: "n8n_erro_http",
                    mensagem: $"O n8n respondeu com HTTP {(int)resHttp.StatusCode}."
                );
            }

            var resN8n = await resHttp.Content.ReadFromJsonAsync<ConsultarAgendaN8nResponse>(cancellationToken: cancellationToken) 
            ?? throw new IntegracaoExternaException(
                codigo: "n8n_resposta_vazia",
                mensagem: "O n8n retornou uma resposta vazia."
            );

            if(!resN8n.Sucesso)
            {
                throw new IntegracaoExternaException(
                    codigo: "n8n_consulta_rejeitada",
                    mensagem: "O n8n não conseguiu consultar a agenda."
                );
            }
            return MapearEventos(
                resN8n,
                profissionalUuid
            );
        }
        catch (IntegracaoExternaException)
        {
            throw;
        }
        catch (OperationCanceledException exception)
            when (!cancellationToken.IsCancellationRequested)
        {
            throw new IntegracaoExternaException(
                codigo: "n8n_timeout",
                mensagem: "O n8n demorou mais que o previsto para responder.",
                innerException: exception
            );
        }
        catch(HttpRequestException exception)
        {
            throw new IntegracaoExternaException(
                codigo: "n8n_indisponivel",
                mensagem: "Não foi possível se conectar ao n8n.",
                innerException: exception
            );
        }
        catch(JsonException exception)
        {
            throw new IntegracaoExternaException(
                codigo: "n8n_resposta_invalida",
                mensagem: "O n8n retornou um JSON inválido.",
                innerException: exception
            );
        }
    }

    private static IReadOnlyCollection<EventoAgendaExterna> MapearEventos(
        ConsultarAgendaN8nResponse res,
        Guid profissionalUuid
    )
    {
        var agendamentos = res.Agendamentos ?? Array.Empty<AgendamentoN8nResponse>();

        return agendamentos.Select(agendamento =>
        {
            if(string.IsNullOrWhiteSpace(agendamento.Id))
            {
                throw new IntegracaoExternaException(
                    codigo: "n8n_evento_invalido",
                    mensagem: "O n8n retornou um evento sem identificador."
                );
            }

            if(agendamento.Fim <= agendamento.Inicio)
            {
                throw new IntegracaoExternaException(
                    codigo: "n8n_periodo_invalido",
                    mensagem: "O n8n retornou um evento com periodo inválido."
                );
            }

            var resProfissionalUuid = agendamento.ExtendedProps?.ProfissionalUuid;
            if(resProfissionalUuid.HasValue && resProfissionalUuid.Value != profissionalUuid)
            {
                throw new IntegracaoExternaException(
                    codigo: "n8n_profissional_invalido",
                    mensagem: "O n8n retornou um evento de outro profissional."
                );
            }

            var resNomeCliente = agendamento.ExtendedProps?.Cliente;
            if(string.IsNullOrWhiteSpace(resNomeCliente))
            {
                resNomeCliente = agendamento.Titulo;
            }

            return new EventoAgendaExterna(
                IdExterno: agendamento.Id,
                Titulo: agendamento.Titulo,
                Inicio: agendamento.Inicio,
                Fim: agendamento.Fim,
                NomeCliente: resNomeCliente,
                NomeProcedimento: agendamento.ExtendedProps?.Procedimento,
                ProfissionalUuid: profissionalUuid
            );
        }).ToArray();
    }
}