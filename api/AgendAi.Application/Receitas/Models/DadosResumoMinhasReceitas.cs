namespace AgendAi.Application.Receitas.Models;

public sealed record DadosResumoMinhasReceitas(
    decimal ReceitaRecebida,
    decimal ReceitaEstimada,
    decimal? ReceitaRecebidaMesAnterior,
    IReadOnlyCollection<DadosMinhasReceitasPorProcedimento> PorProcedimento
);