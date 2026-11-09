namespace AgendAi.Application.Profissionais.Models;

public sealed record DadosAlteracaoVinculoProcedimentoProfissional(
    Guid ClinicaUuid,
    Guid ProfissionalProcedimentoUuid,
    int DuracaoMinutos,
    decimal Valor,
    DateTime? DeletedAt
);