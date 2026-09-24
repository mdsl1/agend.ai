namespace AgendAi.Application.Auth.MeuPerfil;

using AgendAi.Application.Auth.Models;
using AgendAi.Application.Auth.Permissions;
using AgendAi.Application.Auth.Ports;
using AgendAi.Application.Common.Exceptions;
using AgendAi.Domain.Usuarios;

public sealed class MeuPerfilHandler
{
    private readonly IContextUsuarioAtual _context;
    private readonly IMeuPerfilReader _reader;

    public MeuPerfilHandler(
        IContextUsuarioAtual context,
        IMeuPerfilReader reader
    )
    {
        _context = context;
        _reader = reader;
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

        var permissoes = MontarPermissoes(dados);

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
                Nome: dados.ClinicaNome
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

    private static IReadOnlyCollection<string> MontarPermissoes(DadosMeuPerfil dados)
    {
        var permissoes = new List<string>();

        if (dados.ProfissionalUuid.HasValue)
        {
            permissoes.Add(PermissoesUsuario.AgendaPropriaVisualizar);

            permissoes.Add(PermissoesUsuario.AgendamentosPropriosGerenciar);

            permissoes.Add(PermissoesUsuario.IndisponibilidadesPropriasGerenciar);

            permissoes.Add(PermissoesUsuario.ProcedimentosPropriosGerenciar);

            permissoes.Add(PermissoesUsuario.ReceitasPropriasVisualizar);
        }

        if (string.Equals( dados.Cargo, CargosUsuario.Recepcionista, StringComparison.Ordinal))
        {
            permissoes.Add(PermissoesUsuario.AgendaClinicaVisualizar);

            permissoes.Add(PermissoesUsuario.AgendamentosClinicaGerenciar);

            permissoes.Add(PermissoesUsuario.ClientesClinicaGerenciar);

            permissoes.Add(PermissoesUsuario.ProcedimentosClinicaGerenciar);

            permissoes.Add(PermissoesUsuario.EspecialidadesClinicaGerenciar);

            permissoes.Add(PermissoesUsuario.ProfissionaisClinicaGerenciar);
        }

        if (dados.IsAdmin)
        {
            permissoes.Add(PermissoesUsuario.ClinicaGerenciar);

            permissoes.Add(PermissoesUsuario.UsuariosClinicaGerenciar);
        }

        return permissoes.ToArray();
    }
}