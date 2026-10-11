namespace AgendAi.Application.Receitas.Models;

public sealed record DadosMinhasReceitasPorProcedimento(
    Guid ProcedimentoUuid,
    string Nome,
    int Qtde,
    decimal ValorTotal
);