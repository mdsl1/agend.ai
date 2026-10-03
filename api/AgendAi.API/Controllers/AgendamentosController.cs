namespace AgendAi.API.Controllers;

using AgendAi.API.Contracts.Agendamentos;
using AgendAi.Application.Agenda.CriarAgendamento;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AgendAi.Application.Agenda.ConsultarAgendamento;

[ApiController]
[Authorize]
[Route("api/agendamentos")]
public sealed class AgendamentosController : ControllerBase
{
    private readonly CriarAgendamentoHandler _criarAgendamentoHandler;
    private readonly ConsultarAgendamentoHandler _consultarAgendamentoHandler;

    public AgendamentosController(
        CriarAgendamentoHandler criarAgendamentoHandler,
        ConsultarAgendamentoHandler consultarAgendamentoHandler
    )
    {
        _criarAgendamentoHandler = criarAgendamentoHandler;
        _consultarAgendamentoHandler = consultarAgendamentoHandler;
    }

    [HttpGet("{agendamentoUuid:guid}")]
    [ProducesResponseType( typeof(ConsultarAgendamentoResponse), StatusCodes.Status200OK )]
    [ProducesResponseType( typeof(ProblemDetails), StatusCodes.Status400BadRequest )]
    [ProducesResponseType( typeof(ProblemDetails), StatusCodes.Status404NotFound )]
    [ProducesResponseType( StatusCodes.Status401Unauthorized )]
    [ProducesResponseType( typeof(ProblemDetails), StatusCodes.Status403Forbidden )]
    public async Task<ActionResult<ConsultarAgendamentoResponse>> ConsultarAgendamentoAsync(
        [FromRoute] Guid agendamentoUuid,
        CancellationToken cancellationToken
    )
    {
        var query = new ConsultarAgendamentoQuery(AgendamentoUuid: agendamentoUuid);

        var result = await _consultarAgendamentoHandler.HandleAsync( 
            query, 
            cancellationToken 
        );

        var res = new ConsultarAgendamentoResponse(
            Uuid: result.Uuid,
            Cliente: new ClienteAgendamentoResponse(
                result.Cliente.ClienteUuid,
                result.Cliente.NomeCliente,
                result.Cliente.Telefone
            ),
            Profissional: new ProfissionalAgendamentoResponse(
                result.Profissional.ProfissionalUuid,
                result.Profissional.NomeExibicao
            ),
            Procedimento: new ProcedimentoAgendamentoResponse(
                result.Procedimento.ProcedimentoUuid,
                result.Procedimento.NomeProcedimento,
                result.Procedimento.DuracaoMinutos
            ),
            Inicio: result.Inicio,
            Fim: result.Fim,
            MotivoContato: result.MotivoContato,
            AnotacoesProfissional: result.AnotacoesProfissional,
            ValorTotal: result.ValorTotal,
            Status: result.Status,
            StatusPagamento: result.StatusPagamento
        );

        return Ok(res);
    }

    [HttpPost]
    [ProducesResponseType( typeof(CriarAgendamentoResponse), StatusCodes.Status201Created )]
    [ProducesResponseType( typeof(ProblemDetails), StatusCodes.Status400BadRequest )]
    [ProducesResponseType( typeof(ProblemDetails), StatusCodes.Status404NotFound )]
    [ProducesResponseType( typeof(ProblemDetails), StatusCodes.Status409Conflict )]
    [ProducesResponseType( typeof(ProblemDetails), StatusCodes.Status502BadGateway )]
    [ProducesResponseType( StatusCodes.Status401Unauthorized )]
    [ProducesResponseType( typeof(ProblemDetails), StatusCodes.Status403Forbidden )]
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