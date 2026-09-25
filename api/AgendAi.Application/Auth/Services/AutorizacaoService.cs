namespace AgendAi.Application.Auth.Services;

using AgendAi.Application.Auth.Models;
using AgendAi.Application.Auth.Permissions;
using AgendAi.Application.Common.Exceptions;
using AgendAi.Domain.Usuarios;

public sealed class AutorizacaoService
{
    public IReadOnlyCollection<string> ObterPermissoes(DadosUsuarioAtual usuario)
    {
        ArgumentNullException.ThrowIfNull(usuario);

        var permissoes = new List<string>();

        if (usuario.ProfissionalUuid.HasValue)
        {
            permissoes.Add(PermissoesUsuario.AgendaPropriaVisualizar);

            permissoes.Add(PermissoesUsuario.AgendamentosPropriosGerenciar);

            permissoes.Add(PermissoesUsuario.IndisponibilidadesPropriasGerenciar);

            permissoes.Add(PermissoesUsuario.ProcedimentosPropriosGerenciar);

            permissoes.Add(PermissoesUsuario.ReceitasPropriasVisualizar);
        }

        if (string.Equals( usuario.Cargo, CargosUsuario.Recepcionista, StringComparison.Ordinal))
        {
            permissoes.Add(PermissoesUsuario.AgendaClinicaVisualizar);

            permissoes.Add(PermissoesUsuario.AgendamentosClinicaGerenciar);

            permissoes.Add(PermissoesUsuario.ClientesClinicaGerenciar);

            permissoes.Add(PermissoesUsuario.ProcedimentosClinicaGerenciar);

            permissoes.Add(PermissoesUsuario.EspecialidadesClinicaGerenciar);

            permissoes.Add(PermissoesUsuario.ProfissionaisClinicaGerenciar);
        }

        if (usuario.IsAdmin)
        {
            permissoes.Add(PermissoesUsuario.ClinicaGerenciar);

            permissoes.Add(PermissoesUsuario.UsuariosClinicaGerenciar);
        }

        return permissoes.ToArray();
    }

    public bool PossuiPermissao(
        DadosUsuarioAtual usuario,
        string permissao
    )
    {
        return ObterPermissoes(usuario).Contains(
            permissao, 
            StringComparer.Ordinal
        );
    }

    public void ExigirPermissao(
        DadosUsuarioAtual usuario,
        string permissao
    )
    {
        if (!PossuiPermissao(usuario, permissao))
        {
            throw new AcessoNegadoException();
        }
    }

    public void ExigirMesmaClinica(
        DadosUsuarioAtual usuario,
        Guid clinicaUuid
    )
    {
        if (usuario.ClinicaUuid != clinicaUuid)
        {
            throw new AcessoNegadoException();
        }
    }

    public void ExigirAcessoAoProfissional(
        DadosUsuarioAtual usuario,
        Guid clinicaUuid,
        Guid profissionalUuid,
        string permissaoPropria,
        string permissaoClinica
    )
    {
        ExigirMesmaClinica(usuario, clinicaUuid);

        var possuiAcessoProprio = 
            usuario.ProfissionalUuid == profissionalUuid 
            && PossuiPermissao(usuario, permissaoPropria);
        
        var possuiAcessoClinica = PossuiPermissao(usuario, permissaoClinica);

        if (!possuiAcessoProprio && !possuiAcessoClinica)
        {
            throw new AcessoNegadoException();
        }
    }
}