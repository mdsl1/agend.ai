namespace AgendAi.Application.Profissionais.ListarProfissionais;

using AgendAi.Application.Profissionais.Ports;
using AgendAi.Application.Common.Exceptions;
using AgendAi.Application.Auth.Permissions;
using AgendAi.Application.Auth.Ports;
using AgendAi.Application.Auth.Services;

public sealed class ListarProfissionaisHandler {
    private readonly IProfissionaisReader _profissionaisReader;
    private readonly IContextUsuarioAtual _context;
    private readonly AutorizacaoService _autorizacaoService;

    public ListarProfissionaisHandler(
        IProfissionaisReader profissionaisReader,
        IContextUsuarioAtual context,
        AutorizacaoService autorizacaoService
    )
    {
        _profissionaisReader = profissionaisReader;
        _context = context;
        _autorizacaoService = autorizacaoService;
    }

    public async Task<ListarProfissionaisResult> HandleAsync(
        ListarProfissionaisQuery query,
        CancellationToken cancellationToken
    )
    {
        ArgumentNullException.ThrowIfNull(query);

        ValidarQuery(query);

        var usuarioAtual = _context.Obter();

        var podeListarClinica = _autorizacaoService.PossuiPermissao( usuarioAtual, PermissoesUsuario.AgendaClinicaVisualizar );

        var podeListarProprio = 
            usuarioAtual.ProfissionalUuid.HasValue
            && _autorizacaoService.PossuiPermissao( usuarioAtual, PermissoesUsuario.AgendaPropriaVisualizar );

        if (!podeListarClinica && !podeListarProprio)
        {
            throw new AcessoNegadoException();
        }

        Guid? profissionalUuidFiltro = podeListarClinica ? null : usuarioAtual.ProfissionalUuid;

        var dadosProfissionais = await _profissionaisReader.ListarAsync(
            usuarioAtual.ClinicaUuid,
            query.EspecialidadeUuid,
            profissionalUuidFiltro,
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