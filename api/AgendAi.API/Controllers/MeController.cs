namespace AgendAi.API.Controllers;

using AgendAi.API.Contracts.Auth;
using AgendAi.API.Contracts.Overview;
using AgendAi.API.Contracts.Profissionais;
using AgendAi.API.Contracts.Receitas;
using AgendAi.Application.Auth.MeuPerfil;
using AgendAi.Application.Overview.ConsultarMeuOverview;
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
    private readonly ConsultarMeuOverviewHandler _consultarMeuOverviewHandler;

    public MeController(
        MeuPerfilHandler meuPerfilHandler,
        ListarMeusProcedimentosHandler listarMeusProcedimentosHandler,
        ConsultarResumoMinhasReceitasHandler consultarResumoMinhasReceitasHandler,
        ConsultarMeuOverviewHandler consultarMeuOverviewHandler
    )
    {
        _meuPerfilHandler = meuPerfilHandler;
        _listarMeusProcedimentosHandler = listarMeusProcedimentosHandler;
        _consultarResumoMinhasReceitasHandler = consultarResumoMinhasReceitasHandler;
        _consultarMeuOverviewHandler = consultarMeuOverviewHandler;
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

    [HttpGet("overview")]
    [ProducesResponseType( typeof(MeuOverviewResponse), StatusCodes.Status200OK )]
    [ProducesResponseType( typeof(ProblemDetails), StatusCodes.Status400BadRequest )]
    [ProducesResponseType( typeof(ProblemDetails), StatusCodes.Status403Forbidden )]
    [ProducesResponseType( typeof(ProblemDetails), StatusCodes.Status401Unauthorized )]
    public async Task<ActionResult<MeuOverviewResponse>> ConsultarMeuOverviewAsync(
        CancellationToken cancellationToken
    )
    {
        var result = await _consultarMeuOverviewHandler.HandleAsync(
            new ConsultarMeuOverviewQuery(),
            cancellationToken
        );

        var agenda = result.ResumoAgenda;
        var proximoAgendamento = agenda.ProximoAgendamento is not null ?
            new OverviewAgendaResponse.ProximoAgendamentoResponse(
                AgendamentoUuid: agenda.ProximoAgendamento.AgendamentoUuid,
                Inicio: agenda.ProximoAgendamento.Inicio,
                Fim: agenda.ProximoAgendamento.Fim,
                NomeCliente: agenda.ProximoAgendamento.NomeCliente,
                NomeProcedimento: agenda.ProximoAgendamento.NomeProcedimento,
                Status: agenda.ProximoAgendamento.Status
            )
            : null;
        var agendaResponse = new OverviewAgendaResponse(
            QtdeHoje: agenda.QtdeHoje,
            QtdeConfirmados: agenda.QtdeConfirmados,
            QtdeRestante: agenda.QtdeRestante,
            QtdeCancelados: agenda.QtdeCancelados,
            ProximoAgendamento: proximoAgendamento
        );

        var receitas = result.ResumoReceitas;
        var receitasResponse = new OverviewReceitasResponse(
            Periodo: new OverviewReceitasResponse.PeriodoResponse(
                Inicio: receitas.Periodo.Inicio,
                Fim: receitas.Periodo.Fim
            ),
            ValorTotal: receitas.ValorTotal,
            PercentualVsMesAnterior: receitas.PercentualVsMesAnterior
        );

        var res = new MeuOverviewResponse(
            DataReferencia: result.DataReferencia,
            ResumoReceitas: receitasResponse,
            ResumoAgenda: agendaResponse
        );

        return Ok(res);
    }

}