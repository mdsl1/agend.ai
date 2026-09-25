namespace AgendAi.Application.Auth.MeuPerfil;

using AgendAi.Application.Auth.Models;
using AgendAi.Application.Auth.Services;
using AgendAi.Application.Auth.Ports;
using AgendAi.Application.Common.Exceptions;

public sealed class MeuPerfilHandler
{
    private readonly IContextUsuarioAtual _context;
    private readonly IMeuPerfilReader _reader;
    private readonly AutorizacaoService _autorizacaoService;

    public MeuPerfilHandler(
        IContextUsuarioAtual context,
        IMeuPerfilReader reader,
        AutorizacaoService autorizacaoService
    )
    {
        _context = context;
        _reader = reader;
        _autorizacaoService = autorizacaoService;
    }

    public async Task<MeuPerfilResult> HandleAsync(
        CancellationToken cancellationToken
    )
    {
        var usuarioAtual = _context.Obter();

        var dados = await _reader.ObterAsync(
            usuarioAtual.UsuarioUuid,
            usuarioAtual.ClinicaUuid,
            cancellationToken
        );

        if (dados is null)
        {
            throw new NaoAutenticadoException();
        }

        ValidarContextAtual(usuarioAtual, dados);

        var permissoes = _autorizacaoService.ObterPermissoes(usuarioAtual);

        return new MeuPerfilResult(
            UsuarioUuid: dados.UsuarioUuid,
            ProfissionalUuid: dados.ProfissionalUuid,
            Nome: dados.Nome,
            Prefixo: dados.Prefixo,
            Email: dados.Email,
            Cargo: dados.Cargo,
            IsAdmin: dados.IsAdmin,
            RegistroProfissional: dados.RegistroProfissional,
            Especialidade: dados.Especialidade,
            Clinica: new ClinicaMeuPerfilResult(
                Uuid: dados.ClinicaUuid,
                Nome: dados.ClinicaNome,
                TipoClinica: dados.TipoClinica
            ),
            Permissoes: permissoes
        );
    }

    private static void ValidarContextAtual(
        DadosUsuarioAtual usuarioAtual,
        DadosMeuPerfil dados
    )
    {
        var contextMudou = 
            usuarioAtual.UsuarioUuid != dados.UsuarioUuid
            || usuarioAtual.ClinicaUuid != dados.ClinicaUuid
            || !string.Equals(usuarioAtual.Cargo, dados.Cargo, StringComparison.Ordinal)
            || usuarioAtual.IsAdmin != dados.IsAdmin
            || usuarioAtual.ProfissionalUuid != dados.ProfissionalUuid;
        
        if (contextMudou)
        {
            throw new NaoAutenticadoException();
        }
    }
}