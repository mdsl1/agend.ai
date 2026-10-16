namespace AgendAi.API.Controllers;

using AgendAi.API.Contracts.Clinicas;
using AgendAi.Application.Clinicas.ResolverClinica;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/clinicas")]
public sealed class ClinicasController : ControllerBase
{
    private readonly ResolverClinicaHandler _resolverClinicaHandler;

    public ClinicasController(
        ResolverClinicaHandler resolverClinicaHandler
    )
    {
        _resolverClinicaHandler = resolverClinicaHandler;
    }

    [HttpGet("resolver")]
    [ProducesResponseType( typeof(ResolverClinicaResponse), StatusCodes.Status200OK )]
    [ProducesResponseType( typeof(ProblemDetails), StatusCodes.Status400BadRequest )]
    [ProducesResponseType( typeof(ProblemDetails), StatusCodes.Status404NotFound )]
    public async Task<ActionResult<ResolverClinicaResponse>> ResolverAsync(
        [FromQuery] ResolverClinicaRequest req,
        CancellationToken cancellationToken
    )
    {
        var query = new ResolverClinicaQuery(CodigoIdentificador: req.CodigoIdentificador);

        var result = await _resolverClinicaHandler.ResolverClinicaAsync(
            query,
            cancellationToken
        );

        var res = new ResolverClinicaResponse(
            Uuid: result.Uuid,
            Nome: result.Nome
        );

        return Ok(res);
    }
}
