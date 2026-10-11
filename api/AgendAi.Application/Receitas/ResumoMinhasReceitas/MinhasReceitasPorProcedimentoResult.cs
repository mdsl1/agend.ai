namespace AgendAi.Application.Receitas.ResumoMinhasReceitas;

public sealed record MinhasReceitasPorProcedimentoResult(
    Guid ProcedimentoUuid,
    string Nome,
    int Qtde,
    decimal ValorTotal
);