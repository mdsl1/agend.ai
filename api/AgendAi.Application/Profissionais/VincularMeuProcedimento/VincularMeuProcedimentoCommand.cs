namespace AgendAi.Application.Profissionais.VincularMeuProcedimento;

public sealed record VincularMeuProcedimentoCommand(
    Guid ProcedimentoUuid,
    int? DuracaoMinutos,
    decimal? Valor
);