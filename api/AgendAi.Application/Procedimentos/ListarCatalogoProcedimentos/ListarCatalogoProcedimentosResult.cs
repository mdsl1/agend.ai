namespace AgendAi.Application.Procedimentos.ListarCatalogoProcedimentos;

public sealed record ListarCatalogoProcedimentosResult(
    IReadOnlyCollection<ProcedimentoCatalogadoResult> Procedimentos
);