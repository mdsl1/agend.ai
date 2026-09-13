namespace AgendAi.API.Controllers;

using AgendAi.API.Contracts.Clientes;
using AgendAi.Application.Clientes.ResolverCliente;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/clientes")]
public sealed class ClientesController : ControllerBase
{
    private readonly ResolverClienteHandler _resolverClienteHandler;

    public ClientesController( ResolverClienteHandler resolverClienteHandler)
    {
        _resolverClienteHandler = resolverClienteHandler;
    }

    [HttpPost("resolver")]
    [ProducesResponseType(
        typeof(ResolverClienteResponse),
        StatusCodes.Status200OK
    )]
    [ProducesResponseType(
        typeof(ResolverClienteResponse),
        StatusCodes.Status201Created
    )]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status400BadRequest
    )]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status404NotFound
    )]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status409Conflict
    )]
    public async Task<ActionResult<ResolverClienteResponse>> ResolverAsync(
        [FromQuery] Guid clinicaUuid,
        [FromBody] ResolverClienteRequest req,
        CancellationToken cancellationToken
    )
    {
        var command = new ResolverClienteCommand(
            ClinicaUuid: clinicaUuid,
            Nome: req.Nome,
            Telefone: req.Telefone,
            TelegramUserId: req.TelegramUserId
        );

        var result = await _resolverClienteHandler.HandleAsync(
            command,
            cancellationToken
        );

        var res = new ResolverClienteResponse(
            ClienteUuid: result.ClienteUuid,
            Criado: result.Criado,
            Nome: result.Nome,
            Telefone: result.Telefone
        );

        if (res.Criado)
        {
            return StatusCode(
                StatusCodes.Status201Created,
                res
            );
        }

        return Ok(res);
    }


}