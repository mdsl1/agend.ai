namespace AgendAi.Application.Profissionais.ListarProfissionais;

using AgendAi.Application.Profissionais.Ports;
using AgendAi.Application.Common.Exceptions;

public sealed class ListarProfissionaisHandler {
    private readonly IProfissionaisReader _profissionaisReader;

    public ListarProfissionaisHandler(IProfissionaisReader profissionaisReader)
    {
        _profissionaisReader = profissionaisReader;
    }

    public async Task<ListarProfissionaisResult> HandleAsync(
        ListarProfissionaisQuery query,
        CancellationToken cancellationToken
    )
    {
        ArgumentNullException.ThrowIfNull(query);

        ValidarQuery(query);

        var dadosProfissionais = await _profissionaisReader.ListarAsync(
            query.ClinicaUuid,
            query.EspecialidadeUuid,
            cancellationToken
        );

        var profissionais = dadosProfissionais
        .Select(dados => new ProfissionalAgendavelResult(
            ProfissionalUuid: dados.ProfissionalUuid,
            NomeExibicao: SetNomeExibicao(dados.Prefixo, dados.Nome),
            Especialidade: dados.EspecialidadeUuid.HasValue && !string.IsNullOrWhiteSpace(dados.EspecialidadeNome) 
                ? new EspecialidadeProfissionalResult(
                    Uuid: dados.EspecialidadeUuid.Value,
                    Nome: dados.EspecialidadeNome
                ) 
                : null
        ))
        .OrderBy(profissional => profissional.NomeExibicao)
        .ToArray();

        return new ListarProfissionaisResult(profissionais);
    }   

    private static void ValidarQuery(ListarProfissionaisQuery query)
    {
        if(query.ClinicaUuid ==Guid.Empty)
        {
            throw new ValidacaoException(
                codigo: "clinica_uuid_invalido",
                mensagem: "O UUID da clínica é obrigatório."
            );
        }

        if (query.EspecialidadeUuid.HasValue && query.EspecialidadeUuid.Value == Guid.Empty)
        {
            throw new ValidacaoException(
                codigo: "especialidade_uuid_invalido",
                mensagem: "O UUID da especialidade informado é inválido."
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
        
        return $"{prefixo.Trim()} {nomeExibicao}";
    }

}