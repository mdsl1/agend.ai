using AgendAi.API.Contracts.Agenda;
using AgendAi.Application.Agenda.ConsultarAgenda;
using Microsoft.AspNetCore.Mvc;

namespace AgendAi.API.Controllers;

[ApiController]
[Route("api/agenda")]
public sealed class AgendaController : ControllerBase
{
    private readonly ConsultarAgendaHandler _consultarAgendaHandler;

    public AgendaController (
        ConsultarAgendaHandler consultarAgendaHandler
    )
    {
        _consultarAgendaHandler = consultarAgendaHandler;
    }

    [HttpGet("{profissionalUuid:guid}")]
    [ProducesResponseType( typeof(ConsultarAgendaResponse), StatusCodes.Status200OK )]
    [ProducesResponseType( typeof(ProblemDetails), StatusCodes.Status400BadRequest )]
    [ProducesResponseType( typeof(ProblemDetails), StatusCodes.Status404NotFound )]
    [ProducesResponseType( typeof(ProblemDetails), StatusCodes.Status409Conflict )]
    [ProducesResponseType( typeof(ProblemDetails), StatusCodes.Status502BadGateway )]
    public async Task<ActionResult<ConsultarAgendaResponse>> ConsultarAsync(
        [FromRoute] Guid profissionalUuid,
        [FromQuery] ConsultarAgendaRequest req,
        CancellationToken cancellationToken
    )
    {
        var query = new ConsultarAgendaQuery(
            ProfissionalUuid: profissionalUuid,
            Inicio: req.Inicio,
            Fim: req.Fim
        );

        var result = await _consultarAgendaHandler.HandleAsync(query, cancellationToken);

        var eventos = result.Eventos.Select(evento => new EventoAgendaResponse(
            Id: evento.Id,
            Titulo: evento.Titulo,
            Tipo: evento.Tipo,
            AgendamentoUuid: evento.AgendamentoUuid,
            Inicio: evento.Inicio,
            Fim: evento.Fim,
            NomeCliente: evento.NomeCliente,
            NomeProcedimento: evento.NomeProcedimento,
            ProfissionalUuid: evento.ProfissionalUuid
        )).ToArray();

        var res = new ConsultarAgendaResponse(eventos);
        return Ok(res);
    }

}