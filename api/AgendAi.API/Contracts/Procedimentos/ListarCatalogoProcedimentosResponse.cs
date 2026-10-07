namespace AgendAi.API.Contracts.Procedimentos;

public sealed record ListarCatalogoProcedimentosResponse(
    IReadOnlyCollection<ProcedimentoCatalogadoResponse> Procedimentos
);