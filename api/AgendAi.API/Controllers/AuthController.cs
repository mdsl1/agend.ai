namespace AgendAi.API.Controllers;

using AgendAi.API.Contracts.Auth;
using AgendAi.Application.Auth.Login;
using AgendAi.Infrastructure.Mappings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly LoginHandler _loginHandler;
    public AuthController(LoginHandler loginHandler)
    {
        _loginHandler = loginHandler;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType(
        typeof(LoginResponse),
        StatusCodes.Status200OK
    )]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status400BadRequest
    )]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status401Unauthorized
    )]
    public async Task<ActionResult<LoginResponse>> LoginAsync(
        [FromBody] LoginRequest req,
        CancellationToken cancellationToken
    )
    {
        var command = new LoginCommand(
            ClinicaUuid: req.ClinicaUuid,
            Email: req.Email,
            Senha: req.Senha
        );

        var result = await _loginHandler.HandleAsync(
            command,
            cancellationToken
        );

        var usuario = new UsuarioAutenticadoResponse(
            Uuid: result.Usuario.Uuid,
            Nome: result.Usuario.Nome,
            Cargo: result.Usuario.Cargo,
            IsAdmin: result.Usuario.IsAdmin,
            ProfissionalUuid: result.Usuario.ProfissionalUuid,
            ClinicaUuid: result.Usuario.ClinicaUuid
        );

        var res = new LoginResponse(
            AccessToken: result.AccessToken,
            ExpiraEm: result.ExpiraEm,
            Usuario: usuario
        );

        return Ok(res);
    }
}