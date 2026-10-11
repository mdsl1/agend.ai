namespace AgendAi.API.Contracts.Receitas;

public sealed record ResumoMinhasReceitasRequest(
    int Mes,
    int Ano
);