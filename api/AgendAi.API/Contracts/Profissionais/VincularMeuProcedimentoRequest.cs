namespace AgendAi.API.Contracts.Profissionais;

public sealed record VincularMeuProcedimentoRequest(
    Guid ProcedimentoUuid,
    int? DuracaoMinutos,
    decimal? Valor
);