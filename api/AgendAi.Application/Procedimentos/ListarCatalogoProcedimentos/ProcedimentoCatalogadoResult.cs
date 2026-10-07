namespace AgendAi.Application.Procedimentos.ListarCatalogoProcedimentos;

public sealed record ProcedimentoCatalogadoResult(
    Guid ProcedimentoUuid,
    string NomeProcedimento,
    int DuracaoEstimadaMinutos,
    decimal ValorBase
);