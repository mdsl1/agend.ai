namespace AgendAi.API.Controllers;

using AgendAi.API.Contracts.Auth;
using AgendAi.API.Contracts.Profissionais;
using AgendAi.API.Contracts.Receitas;
using AgendAi.Application.Auth.MeuPerfil;
using AgendAi.Application.Profissionais.ListarMeusProcedimentos;
using AgendAi.Application.Receitas.ResumoMinhasReceitas;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Authorize]
[Route("api/me")]
public sealed class MeController : ControllerBase
{
    private readonly MeuPerfilHandler _meuPerfilHandler;
    private readonly ListarMeusProcedimentosHandler _listarMeusProcedimentosHandler;
    private readonly ConsultarResumoMinhasReceitasHandler _consultarResumoMinhasReceitasHandler;

    public MeController(
        MeuPerfilHandler meuPerfilHandler,
        ListarMeusProcedimentosHandler listarMeusProcedimentosHandler,
        ConsultarResumoMinhasReceitasHandler consultarResumoMinhasReceitasHandler
    )
    {
        _meuPerfilHandler = meuPerfilHandler;
        _listarMeusProcedimentosHandler = listarMeusProcedimentosHandler;
        _consultarResumoMinhasReceitasHandler = consultarResumoMinhasReceitasHandler;
    }

    [HttpGet]
    [ProducesResponseType( typeof(MeuPerfilResponse), StatusCodes.Status200OK )]
    [ProducesResponseType( StatusCodes.Status401Unauthorized )]
    public async Task<ActionResult<MeuPerfilResponse>> ObterAsync(
        CancellationToken cancellationToken
    )
    {
        var result = await _meuPerfilHandler.HandleAsync(
            cancellationToken
        );

        var response = new MeuPerfilResponse(
            UsuarioUuid: result.UsuarioUuid,
            ProfissionalUuid: result.ProfissionalUuid,
            Nome: result.Nome,
            Prefixo: result.Prefixo,
            Email: result.Email,
            Cargo: result.Cargo,
            IsAdmin: result.IsAdmin,
            RegistroProfissional: result.RegistroProfissional,
            Especialidade: result.Especialidade,
            Clinica: new ClinicaUsuarioResponse(
                Uuid: result.Clinica.Uuid,
                Nome: result.Clinica.Nome,
                TipoClinica: result.Clinica.TipoClinica
            ),
            Permissoes: result.Permissoes
        );

        return Ok(response);
    }

    [HttpGet("procedimentos")]
    [ProducesResponseType( typeof(ListarMeusProcedimentosResult), StatusCodes.Status200OK )]
    [ProducesResponseType( typeof(ProblemDetails), StatusCodes.Status404NotFound )]
    [ProducesResponseType( StatusCodes.Status401Unauthorized )]
    public async Task<ActionResult<ListarMeusProcedimentosResult>> ListarMeusProcedimentosAsync(
        CancellationToken cancellationToken
    )
    {
        var result = await _listarMeusProcedimentosHandler.HandleAsync(
            new ListarMeusProcedimentosQuery(),
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

        var res = new ListarMeusProcedimentosResponse(procedimentos);

        return Ok(res);
    }

    [HttpGet("receitas/resumo")]
    [ProducesResponseType( typeof(ResumoMinhasReceitasResponse), StatusCodes.Status200OK )]
    [ProducesResponseType( typeof(ProblemDetails), StatusCodes.Status400BadRequest )]
    [ProducesResponseType( typeof(ProblemDetails), StatusCodes.Status403Forbidden )]
    [ProducesResponseType( typeof(ProblemDetails), StatusCodes.Status401Unauthorized )]
    public async Task<ActionResult<ResumoMinhasReceitasResponse>> ConsultarResumoMinhasReceitasAsync(
        [FromQuery] ResumoMinhasReceitasRequest req,
        CancellationToken cancellationToken
    )
    {
        var query = new ConsultarResumoMinhasReceitasQuery(
            Mes: req.Mes,
            Ano: req.Ano
        );

        var result = await _consultarResumoMinhasReceitasHandler.HandleAsync(query, cancellationToken);

        var resumoPorProcedimento = result.PorProcedimento
            .Select(item => new MinhasReceitasPorProcedimentoResponse(
                item.ProcedimentoUuid,
                item.Nome,
                item.Qtde,
                item.ValorTotal
            )).ToArray();

        var res = new ResumoMinhasReceitasResponse(
            Periodo: new ResumoMinhasReceitasResponse.PeriodoResponse(
                Inicio: result.Periodo.Inicio,
                Fim: result.Periodo.Fim
            ),
            ReceitaRecebida: result.ReceitaRecebida,
            ReceitaEstimada: result.ReceitaEstimada,
            PercentualVsMesAnterior: result.PercentualVsMesAnterior,
            PorProcedimento: resumoPorProcedimento
        );

        return Ok(res);
    }
}