namespace AgendAi.API.Contracts.Procedimentos;

public sealed record ProcedimentoCatalogadoResponse(
    Guid Uuid,
    string Nome,
    int DuracaoEstimadaMinutos,
    decimal ValorBase
);