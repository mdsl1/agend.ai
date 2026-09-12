namespace AgendAi.API.Controllers;

using AgendAi.API.Contracts.Agendamentos;
using AgendAi.Application.Agenda.CriarAgendamento;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/agendamentos")]
public sealed class AgendamentosController : ControllerBase
{
    private readonly CriarAgendamentoHandler _criarAgendamentoHandler;

    public AgendamentosController(CriarAgendamentoHandler criarAgendamentoHandler)
    {
        _criarAgendamentoHandler = criarAgendamentoHandler;
    }

    [HttpPost]
    [ProducesResponseType(
        typeof(CriarAgendamentoResponse),
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
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status502BadGateway
    )]
    public async Task<ActionResult<CriarAgendamentoResponse>> CriarAsync(
        [FromBody] CriarAgendamentoRequest req,
        [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey,
        CancellationToken cancellationToken
    )
    {
        var command = new CriarAgendamentoCommand(
            ClienteUuid: req.ClienteUuid,
            ProfissionalProcedimentoUuid: req.ProfissionalProcedimentoUuid,
            Inicio: req.Inicio,
            MotivoContato: req.MotivoContato,
            IdempotencyKey: idempotencyKey ?? string.Empty
        );

        var result = await _criarAgendamentoHandler.HandleAsync(
            command, 
            cancellationToken
        );

        var res = new CriarAgendamentoResponse(
            Uuid: result.AgendamentoUuid,
            Inicio: result.Inicio,
            Fim: result.Fim,
            NomeCliente: result.NomeCliente,
            NomeProfissional: result.NomeProfissional,
            NomeProcedimento: result.NomeProcedimento,
            ValorTotal: result.ValorTotal,
            Status: result.Status
        );

        return StatusCode(
            StatusCodes.Status201Created,
            res
        );
    }
    
}