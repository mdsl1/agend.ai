namespace AgendAi.Application.Procedimentos.Models;

public sealed record DadosProcedimentoCatalogado(
    Guid ClinicaUuid,
    Guid ProcedimentoUuid,
    string NomeProcedimento,
    int DuracaoEstimadaMinutos,
    decimal ValorBase
);