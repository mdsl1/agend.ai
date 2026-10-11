namespace AgendAi.Application.Receitas.ResumoMinhasReceitas;

using AgendAi.Application.Auth.Permissions;
using AgendAi.Application.Auth.Ports;
using AgendAi.Application.Auth.Services;
using AgendAi.Application.Common.Exceptions;
using AgendAi.Application.Receitas.Ports;

public sealed class ConsultarResumoMinhasReceitasHandler
{
    private const string TimezonePadrao = "America/Sao_Paulo";
    private static readonly TimeZoneInfo TimezoneApp = TimeZoneInfo.FindSystemTimeZoneById(TimezonePadrao);

    private readonly IResumoMinhasReceitasReader _reader;
    private readonly IContextUsuarioAtual _context;
    private readonly AutorizacaoService _autorizacaoService;

    public ConsultarResumoMinhasReceitasHandler(
        IResumoMinhasReceitasReader reader,
        IContextUsuarioAtual context,
        AutorizacaoService autorizacaoService
    )
    {
        _reader = reader;
        _context = context;
        _autorizacaoService = autorizacaoService;
    }

    public async Task<ConsultarResumoMinhasReceitasResult> HandleAsync(
        ConsultarResumoMinhasReceitasQuery query,
        CancellationToken cancellationToken
    )
    {
        ArgumentNullException.ThrowIfNull(query);

        ValidarQuery(query);

        var usuarioAtual = _context.Obter();

        _autorizacaoService.ExigirPermissao(
            usuarioAtual,
            PermissoesUsuario.ReceitasPropriasVisualizar
        );

        var profissionalUuid = usuarioAtual.ProfissionalUuid
            ?? throw new AcessoNegadoException();

        if (profissionalUuid == Guid.Empty)
        {
            throw new AcessoNegadoException();
        }

        var inicioMes = new DateOnly(query.Ano, query.Mes, 1);
        var inicioMesAnterior = inicioMes.AddMonths(-1);
        var inicioProximoMes = inicioMes.AddMonths(1);

        var dados = await _reader.ConsultarAsync(
            clinicaUuid: usuarioAtual.ClinicaUuid,
            profissionalUuid: profissionalUuid,
            inicioMesAnterior: ConverterParaUtc(inicioMesAnterior),
            inicioMes: ConverterParaUtc(inicioMes),
            inicioProximoMes: ConverterParaUtc(inicioProximoMes),
            cancellationToken: cancellationToken
        );

        var percentualVsMesAnterior = dados.ReceitaRecebidaMesAnterior is > 0m
            ? (dados.ReceitaRecebida - dados.ReceitaRecebidaMesAnterior.Value) / dados.ReceitaRecebidaMesAnterior.Value
            : (decimal?) null;

        var porProcedimento = dados.PorProcedimento
            .Select(registro => new MinhasReceitasPorProcedimentoResult(
                ProcedimentoUuid: registro.ProcedimentoUuid,
                Nome: registro.Nome,
                ValorTotal: registro.ValorTotal,
                Qtde: registro.Qtde
            )).ToArray();

        return new ConsultarResumoMinhasReceitasResult(
            Periodo: new ConsultarResumoMinhasReceitasResult.PeriodoResult(
                Inicio: inicioMes,
                Fim: inicioProximoMes
            ),
            ReceitaRecebida: dados.ReceitaRecebida,
            ReceitaEstimada: dados.ReceitaEstimada,
            PercentualVsMesAnterior: percentualVsMesAnterior,
            PorProcedimento: porProcedimento
        );
    }

    private static void ValidarQuery(ConsultarResumoMinhasReceitasQuery query)
    {
        if (query.Mes is < 1 or > 12)
        {
            throw new ValidacaoException(
                "mes_invalido",
                "O mês deve estar entre 1 e 12."
            );
        }

        int anoVigente = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimezoneApp).Year;
        var anoMinimoBusca = 2020;
        if (query.Ano < anoMinimoBusca || query.Ano > anoVigente)
        {
            throw new ValidacaoException(
                "ano_invalido",
                $"O ano deve estar entre {anoMinimoBusca} e {anoVigente}."
            );
        }
    }

    private static DateTimeOffset ConverterParaUtc(DateOnly data)
    {
        var inicioLocal = data.ToDateTime(
            TimeOnly.MinValue,
            DateTimeKind.Unspecified
        );

        var inicioUtc = TimeZoneInfo.ConvertTimeToUtc(
            inicioLocal,
            TimezoneApp
        );

        return new DateTimeOffset(inicioUtc);
    }
}