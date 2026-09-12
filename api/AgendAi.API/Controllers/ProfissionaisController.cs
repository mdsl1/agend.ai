namespace AgendAi.API.Controllers;

using AgendAi.API.Contracts.Profissionais;
using AgendAi.Application.Profissionais.ListarProfissionais;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/profissionais")]
public sealed class ProfissionaisController : ControllerBase
{
    private readonly ListarProfissionaisHandler _listarProfissionaisHandler;

    public ProfissionaisController(ListarProfissionaisHandler listarProfissionaisHandler)
    {
        _listarProfissionaisHandler = listarProfissionaisHandler;
    }

    [HttpGet]
    [ProducesResponseType(
        typeof(ListarProfissionaisResponse),
        StatusCodes.Status200OK
    )]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status400BadRequest
    )]
    public async Task<ActionResult<ListarProfissionaisResponse>> ListarAsync(
        [FromQuery] ListarProfissionaisQuery req,
        CancellationToken cancellationToken
    )
    {
        var query = new ListarProfissionaisQuery(
            ClinicaUuid: req.ClinicaUuid,
            EspecialidadeUuid: req.EspecialidadeUuid
        );

        var result = await _listarProfissionaisHandler.HandleAsync(
            query,
            cancellationToken
        );

        var profissionais = result.Profissionais
            .Select(profissional => new ProfissionalAgendavelResponse(
                ProfissionalUuid: profissional.ProfissionalUuid,
                NomeExibicao: profissional.NomeExibicao,
                Especialidade: profissional.Especialidade is null 
                    ? null
                    : new EspecialidadeProfissionalResponse(
                        Uuid: profissional.Especialidade.Uuid,
                        Nome: profissional.Especialidade.Nome
                    )
            )).ToArray();

        var res = new ListarProfissionaisResponse(profissionais);

        return Ok(res);
    }
    
}
