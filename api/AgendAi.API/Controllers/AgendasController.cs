namespace AgendAi.API.Controllers;

using AgendAi.API.Contracts.Agenda;
using AgendAi.Application.Agenda.ListarAgendas;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/agendas")]
public sealed class AgendasController  : ControllerBase
{
    private readonly ListarAgendasHandler _listarAgendasHandler;

    public AgendasController(ListarAgendasHandler listarAgendasHandler)
    {
        _listarAgendasHandler = listarAgendasHandler;
    }

    [HttpGet]
    [ProducesResponseType( typeof(ListarAgendasResponse), StatusCodes.Status200OK )]
    [ProducesResponseType( typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ListarAgendasResponse>> ListarAsync(
        [FromQuery] ListarAgendasRequest request,
        CancellationToken cancellationToken
    )
    {
        var query = new ListarAgendasQuery(ClinicaUuid: request.ClinicaUuid);

        var result = await _listarAgendasHandler.HandleAsync(query, cancellationToken);

        var agendas = result.Agendas.Select(agenda => new AgendaDisponivelResponse(
            ProfissionalUuid: agenda.ProfissionalUuid,
            NomeExibicao: agenda.NomeExibicao
        )).ToArray();

        var res = new ListarAgendasResponse(agendas);
        return Ok(res);
    }

}