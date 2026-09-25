namespace AgendAi.API.Controllers;

using AgendAi.API.Contracts.Profissionais;
using AgendAi.Application.Profissionais.ListarProcedimentosProfissional;
using AgendAi.Application.Profissionais.ListarProfissionais;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

[ApiController]
[Authorize]
[Route("api/profissionais")]
public sealed class ProfissionaisController : ControllerBase
{
    private readonly ListarProfissionaisHandler _listarProfissionaisHandler;
    private readonly ListarProcedimentosProfissionalHandler _listarProcedimentosProfissionalHandler;

    public ProfissionaisController(
        ListarProfissionaisHandler listarProfissionaisHandler,
        ListarProcedimentosProfissionalHandler listarProcedimentosProfissionalHandler
    )
    {
        _listarProfissionaisHandler = listarProfissionaisHandler;
        _listarProcedimentosProfissionalHandler =  listarProcedimentosProfissionalHandler;
    }

    [HttpGet]
    [ProducesResponseType( typeof(ListarProfissionaisResponse), StatusCodes.Status200OK )]
    [ProducesResponseType( typeof(ProblemDetails), StatusCodes.Status400BadRequest )]
    [ProducesResponseType( StatusCodes.Status401Unauthorized )]
    [ProducesResponseType( typeof(ProblemDetails), StatusCodes.Status403Forbidden )]
    public async Task<ActionResult<ListarProfissionaisResponse>> ListarAsync(
        [FromQuery] ListarProfissionaisRequest req,
        CancellationToken cancellationToken
    )
    {
        var query = new ListarProfissionaisQuery( EspecialidadeUuid: req.EspecialidadeUuid );

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

    [HttpGet("{profissionalUuid:guid}/procedimentos")]
    [ProducesResponseType( typeof(ListarProcedimentosProfissionalResponse), StatusCodes.Status200OK )]
    [ProducesResponseType( typeof(ProblemDetails), StatusCodes.Status400BadRequest )]
    [ProducesResponseType( typeof(ProblemDetails), StatusCodes.Status404NotFound )]
    [ProducesResponseType( StatusCodes.Status401Unauthorized )]
    [ProducesResponseType( typeof(ProblemDetails), StatusCodes.Status403Forbidden )]
    public async Task<ActionResult<ListarProcedimentosProfissionalResponse>> ListarProcedimentosAsync(
        [FromRoute] Guid profissionalUuid,
        CancellationToken cancellationToken
    )
    {
        var query = new ListarProcedimentosProfissionalQuery( ProfissionalUuid: profissionalUuid );

        var result = await _listarProcedimentosProfissionalHandler.HandleAsync(
            query,
            cancellationToken
        );
    
        var procedimentos = result.Procedimentos
            .Select(procedimento => new ProcedimentoProfissionalResponse(
                ProfissionalProcedimentoUuid: procedimento.ProfissionalProcedimentoUuid,
                ProcedimentoUuid: procedimento.ProcedimentoUuid,
                Nome: procedimento.Nome,
                ValorEfetivo: procedimento.ValorEfetivo,
                DuracaoEfetivaMinutos: procedimento.DuracaoEfetivaMinutos
            )).ToArray();

        var res = new ListarProcedimentosProfissionalResponse( Procedimentos: procedimentos);

        return Ok(res);
    }
}
