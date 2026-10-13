namespace AgendAi.Application.Overview.ConsultarMeuOverview;

using AgendAi.Application.Auth.Permissions;
using AgendAi.Application.Auth.Ports;
using AgendAi.Application.Auth.Services;
using AgendAi.Application.Common.Exceptions;
using AgendAi.Application.Receitas.Ports;
using AgendAi.Application.Overview.Ports;

public sealed class ConsultarMeuOverviewHandler
{
    private const string TimezonePadrao = "America/Sao_Paulo";
    private static readonly TimeZoneInfo TimezoneApp = TimeZoneInfo.FindSystemTimeZoneById(TimezonePadrao);
    private readonly TimeOnly MinVal = TimeOnly.MinValue;
    private readonly TimeOnly MaxVal = TimeOnly.MaxValue;
    private readonly IResumoMinhasReceitasReader _receitasReader;
    private readonly IResumoAgendaReader _agendaReader;
    private readonly IContextUsuarioAtual _context;
    private readonly AutorizacaoService _autorizacaoService;

    public ConsultarMeuOverviewHandler(
        IResumoMinhasReceitasReader receitasReader,
        IResumoAgendaReader agendaReader,
        IContextUsuarioAtual context,
        AutorizacaoService autorizacaoService
    )
    {
        _receitasReader = receitasReader;
        _agendaReader = agendaReader;
        _context = context;
        _autorizacaoService = autorizacaoService;
    }

    public async Task<ConsultarMeuOverviewResult> HandleAsync(
        ConsultarMeuOverviewQuery query,
        CancellationToken cancellationToken
    )
    {
        ArgumentNullException.ThrowIfNull(query);

        var usuarioAtual = _context.Obter();

        _autorizacaoService.ExigirPermissao(
            usuarioAtual,
            PermissoesUsuario.AgendaPropriaVisualizar);

        _autorizacaoService.ExigirPermissao(
            usuarioAtual,
            PermissoesUsuario.ReceitasPropriasVisualizar);

        var profissionalUuid = usuarioAtual.ProfissionalUuid
            ?? throw new AcessoNegadoException();

        if (profissionalUuid == Guid.Empty)
        {
            throw new AcessoNegadoException();
        }

        var agora = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, TimezoneApp);
        var hoje = DateOnly.FromDateTime(agora.DateTime);

        // Handler Receitas
        var inicioMes = new DateOnly(hoje.Year, hoje.Month, 1);
        var inicioMesAnterior = inicioMes.AddMonths(-1);
        var inicioProximoMes = inicioMes.AddMonths(1);

        var dadosReceitas = await _receitasReader.ConsultarAsync(
            clinicaUuid: usuarioAtual.ClinicaUuid,
            profissionalUuid: profissionalUuid,
            inicioMesAnterior: ConverterParaUtc(inicioMesAnterior, MinVal),
            inicioMes: ConverterParaUtc(inicioMes, MinVal),
            inicioProximoMes: ConverterParaUtc(inicioProximoMes, MinVal),
            cancellationToken: cancellationToken
        );

        var percentualVsMesAnterior = dadosReceitas.ReceitaRecebidaMesAnterior is > 0m
            ? (dadosReceitas.ReceitaRecebida - dadosReceitas.ReceitaRecebidaMesAnterior.Value) / dadosReceitas.ReceitaRecebidaMesAnterior.Value
            : (decimal?) null;

        var valorTotal = dadosReceitas.ReceitaRecebida + dadosReceitas.ReceitaEstimada;

        var resumoReceitas = new OverviewReceitasResult(
            Periodo: new OverviewReceitasResult.PeriodoResult(
                Inicio: inicioMes,
                Fim: inicioProximoMes
            ),
            ValorTotal: valorTotal,
            PercentualVsMesAnterior: percentualVsMesAnterior
        );

        // Handler Agenda
        var dadosAgenda = await _agendaReader.ConsultarAsync(
            clinicaUuid: usuarioAtual.ClinicaUuid,
            profissionalUuid: profissionalUuid,
            inicioDia: ConverterParaUtc(hoje, MinVal),
            fimDia: ConverterParaUtc(hoje, MaxVal),
            agora: agora,
            cancellationToken: cancellationToken
        );

        var proximoAgendamento = dadosAgenda.ProximoAgendamento is not null ?
            new OverviewAgendaResult.ProximoAgendamentoResult(
                AgendamentoUuid: dadosAgenda.ProximoAgendamento.AgendamentoUuid,
                Inicio: dadosAgenda.ProximoAgendamento.Inicio,
                Fim: dadosAgenda.ProximoAgendamento.Fim,
                NomeCliente: dadosAgenda.ProximoAgendamento.NomeCliente,
                NomeProcedimento: dadosAgenda.ProximoAgendamento.NomeProcedimento,
                Status: dadosAgenda.ProximoAgendamento.Status
            )
            : null;

        var resumoAgenda = new OverviewAgendaResult(
            QtdeHoje: dadosAgenda.QtdeHoje,
            QtdeConfirmados: dadosAgenda.QtdeConfirmados,
            QtdeRestante: dadosAgenda.QtdeRestante,
            QtdeCancelados: dadosAgenda.QtdeCancelados,
            ProximoAgendamento: proximoAgendamento
        );

        return new ConsultarMeuOverviewResult(
            DataReferencia: hoje,
            ResumoAgenda: resumoAgenda,
            ResumoReceitas: resumoReceitas
        );
    }

    private static DateTimeOffset ConverterParaUtc(DateOnly data, TimeOnly horario)
    {
        var inicioLocal = data.ToDateTime(
            horario,
            DateTimeKind.Unspecified
        );

        var inicioUtc = TimeZoneInfo.ConvertTimeToUtc(
            inicioLocal,
            TimezoneApp
        );

        return new DateTimeOffset(inicioUtc);
    }
}