namespace AgendAi.Application.Agenda.ListarAgendas;
using AgendAi.Application.Agenda.Models;
using AgendAi.Application.Agenda.Ports;
using AgendAi.Application.Common.Exceptions;

public sealed class ListarAgendasHandler {
    private readonly IAgendasReader _agendasReader;

    public ListarAgendasHandler(IAgendasReader agendasReader)
    {
        _agendasReader = agendasReader;
    }

    public async Task<ListarAgendasResult> HandleAsync(
        ListarAgendasQuery query,
        CancellationToken cancellationToken
    )
    {
        ArgumentNullException.ThrowIfNull(query);

        ValidarQuery(query);

        var dadosAgendas = await _agendasReader.ListarPorClinicaAsync(query.ClinicaUuid, cancellationToken);
        var agendas = dadosAgendas
        .Select(dados => new AgendaDisponivelResult(
            ProfissionalUuid: dados.ProfissionalUuid,
            NomeExibicao: SetNomeExibicao(dados.Prefixo, dados.Nome)
        ))
        .OrderBy(agenda => agenda.NomeExibicao)
        .ToArray();

        return new ListarAgendasResult(agendas);
    }   

    private static void ValidarQuery(ListarAgendasQuery query)
    {
        if(query.ClinicaUuid ==Guid.Empty)
        {
            throw new ValidacaoException(
                codigo: "clinica_uuid_invalido",
                mensagem: "O Uuid da clínica é obrigatório."
            );
        }
    }

    private static string SetNomeExibicao(string? prefixo, string nome)
    {
        string[] nomeTratado = nome.Split(
            ' ',
            StringSplitOptions.RemoveEmptyEntries |
            StringSplitOptions.TrimEntries
        );

        var nomeExibicao = "";
        if(nomeTratado.Length > 2)
        {
            nomeExibicao = $"{nomeTratado[0]} {char.ToUpper(nomeTratado[1][0])}. {nomeTratado[^1]}";
        }
        else
        {
            nomeExibicao = string.Join(' ', nomeTratado);
        }

        if(string.IsNullOrWhiteSpace(prefixo))
        {
            return nomeExibicao;
        }
        
        return $"{prefixo} {nomeExibicao}";
    }

}