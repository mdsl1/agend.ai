namespace AgendAi.API.Contracts.Receitas;

public sealed record MinhasReceitasPorProcedimentoResponse(
    Guid ProcedimentoUuid,
    string Nome,
    int Qtde,
    decimal ValorTotal
);