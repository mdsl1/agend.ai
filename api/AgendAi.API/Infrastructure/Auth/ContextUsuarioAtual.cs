namespace AgendAi.API.Infrastructure.Auth;

using System.Security.Claims;
using AgendAi.Application.Auth.Models;
using AgendAi.Application.Auth.Ports;
using AgendAi.Application.Common.Exceptions;
using AgendAi.Infrastructure.Auth;
using Microsoft.IdentityModel.JsonWebTokens;

public sealed class ContextUsuarioAtual : IContextUsuarioAtual
{
    private readonly IHttpContextAccessor _acessor;

    public ContextUsuarioAtual(IHttpContextAccessor acessor)
    {
        _acessor = acessor;
    }

    public DadosUsuarioAtual Obter()
    {
        var principal = _acessor.HttpContext?.User;

        if (
            principal?.Identity?.IsAuthenticated != true 
            || !Guid.TryParse(principal.FindFirstValue(JwtRegisteredClaimNames.Sub), out var usuarioUuid)
            || !Guid.TryParse(principal.FindFirstValue(JwtClaims.ClinicaUuid), out var clinicaUuid)
        )
        {
            throw new NaoAutenticadoException();
        }

        var cargo = principal.FindFirstValue(JwtClaims.Cargo);
        var isAdminClaim = principal.FindFirstValue(JwtClaims.IsAdmin);

        if (string.IsNullOrWhiteSpace(cargo) || !bool.TryParse(isAdminClaim, out var isAdmin))
        {
            throw new NaoAutenticadoException();
        }

        Guid? profissionalUuid = null;
        var profissionalUuidClaim = principal.FindFirstValue(JwtClaims.ProfissionalUuid);

        if (!string.IsNullOrWhiteSpace(profissionalUuidClaim))
        {
            if (!Guid.TryParse(profissionalUuidClaim, out var profissionalUuidFound))
            {
                throw new NaoAutenticadoException();
            }

            profissionalUuid = profissionalUuidFound;
        }

        return new DadosUsuarioAtual(
            usuarioUuid,
            clinicaUuid,
            cargo,
            isAdmin,
            profissionalUuid
        );
    }
}