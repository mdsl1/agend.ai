namespace AgendAi.API.Controllers;

using AgendAi.API.Contracts.Auth;
using AgendAi.Application.Auth.MeuPerfil;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Authorize]
[Route("api/me")]
public sealed class MeController : ControllerBase
{
    private readonly MeuPerfilHandler _meuPerfilHandler;

    public MeController(
        MeuPerfilHandler meuPerfilHandler
    )
    {
        _meuPerfilHandler = meuPerfilHandler;
    }

    [HttpGet]
    [ProducesResponseType( typeof(MeuPerfilResponse), StatusCodes.Status200OK )]
    [ProducesResponseType( StatusCodes.Status401Unauthorized )]
    public async Task<ActionResult<MeuPerfilResponse>> ObterAsync(
        CancellationToken cancellationToken
    )
    {
        var result = await _meuPerfilHandler.HandleAsync(
            cancellationToken
        );

        var response = new MeuPerfilResponse(
            UsuarioUuid: result.UsuarioUuid,
            ProfissionalUuid: result.ProfissionalUuid,
            Nome: result.Nome,
            Prefixo: result.Prefixo,
            Email: result.Email,
            Cargo: result.Cargo,
            IsAdmin: result.IsAdmin,
            RegistroProfissional: result.RegistroProfissional,
            Especialidade: result.Especialidade,
            Clinica: new ClinicaUsuarioResponse(
                Uuid: result.Clinica.Uuid,
                Nome: result.Clinica.Nome
            ),
            Permissoes: result.Permissoes
        );

        return Ok(response);
    }
}