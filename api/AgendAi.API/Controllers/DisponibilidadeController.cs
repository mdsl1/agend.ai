namespace AgendAi.API.Controllers;

using AgendAi.API.Contracts.Disponibilidade;
using AgendAi.Application.Agenda.ConsultarDisponibilidade;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

[ApiController]
[Authorize]
[Route("/api/profissionais/{profissionalUuid:guid}/disponibilidade")]
public sealed class DisponibilidadeController : ControllerBase
{
    private readonly ConsultarDisponibilidadeHandler _consultarDisponibilidadeHandler;

    public DisponibilidadeController(ConsultarDisponibilidadeHandler consultarDisponibilidadeHandler)
    {
        _consultarDisponibilidadeHandler = consultarDisponibilidadeHandler;
    }

    [HttpGet]
    [ProducesResponseType( typeof(ConsultarDisponibilidadeResponse), StatusCodes.Status200OK )]
    [ProducesResponseType( typeof(ProblemDetails), StatusCodes.Status400BadRequest )]
    [ProducesResponseType( typeof(ProblemDetails), StatusCodes.Status404NotFound )]
    [ProducesResponseType( typeof(ProblemDetails), StatusCodes.Status409Conflict )]
    [ProducesResponseType( typeof(ProblemDetails), StatusCodes.Status502BadGateway )]
    [ProducesResponseType( StatusCodes.Status401Unauthorized )]
    [ProducesResponseType( typeof(ProblemDetails), StatusCodes.Status403Forbidden )]
    public async Task<ActionResult<ConsultarDisponibilidadeResponse>> ConsultarAsync(
        [FromRoute] Guid profissionalUuid,
        [FromQuery] ConsultarDisponibilidadeRequest req,
        CancellationToken cancellationToken
    )
    {
        var query = new ConsultarDisponibilidadeQuery(

            ProfissionalUuid: profissionalUuid,
            ProfissionalProcedimentoUuid: req.ProfissionalProcedimentoUuid,
            Inicio: req.Inicio,
            Limite: req.Limite
        );

        var result = await _consultarDisponibilidadeHandler.HandleAsync(query, cancellationToken);

        var horariosDisponiveis = result.HorariosDisponiveis
        .Select(horario => new HorarioDisponivelResponse(
            Inicio: horario.Inicio,
            Fim: horario.Fim
        )).ToArray();

        var res = new ConsultarDisponibilidadeResponse(
            Disponivel: result.Disponivel,
            DuracaoMinutos: result.DuracaoMinutos,
            HorariosDisponiveis: horariosDisponiveis,
            BuscaEsgotada: result.BuscaEsgotada
        );

        return Ok(res);
    }
}