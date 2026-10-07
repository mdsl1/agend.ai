namespace AgendAi.API.Controllers;

using AgendAi.API.Contracts.Procedimentos;
using AgendAi.Application.Procedimentos.ListarCatalogoProcedimentos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AgendAi.API.Contracts.Agendamentos;

[ApiController]
[Authorize]
[Route("api/procedimentos")]
public sealed class ProcedimentosController : ControllerBase
{
    private readonly ListarCatalogoProcedimentosHandler _listarCatalogoProcedimentosHandler;

    public ProcedimentosController(
        ListarCatalogoProcedimentosHandler listarCatalogoProcedimentosHandler
    )
    {
        _listarCatalogoProcedimentosHandler = listarCatalogoProcedimentosHandler;
    }

    [HttpGet]
    [ProducesResponseType( typeof(ListarCatalogoProcedimentosResponse), StatusCodes.Status200OK )]
    [ProducesResponseType( StatusCodes.Status401Unauthorized)]
    [ProducesResponseType( typeof(ProblemDetails), StatusCodes.Status403Forbidden )]
    public async Task<ActionResult<ListarCatalogoProcedimentosResponse>> ListarCatalogoProcedimentosAsync(CancellationToken cancellationToken)
    {
        var result = await _listarCatalogoProcedimentosHandler.HandleAsync(
            new ListarCatalogoProcedimentosQuery(),
            cancellationToken
        );

        var procedimentos = result.Procedimentos
            .Select(procedimento => new ProcedimentoCatalogadoResponse(
                Uuid: procedimento.ProcedimentoUuid,
                Nome: procedimento.NomeProcedimento,
                DuracaoEstimadaMinutos: procedimento.DuracaoEstimadaMinutos,
                ValorBase: procedimento.ValorBase
            )).ToArray();

        var res = new ListarCatalogoProcedimentosResponse(
            Procedimentos: procedimentos
        );

        return Ok(res);
    }
}