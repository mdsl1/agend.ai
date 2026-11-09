namespace AgendAi.Application.Profissionais.Models;

public sealed record DadosCriacaoVinculoProcedimentoProfissional(
    Guid ClinicaUuid,
    Guid ProfissionalUuid,
    Guid ProcedimentoUuid,
    int DuracaoMinutos,
    decimal Valor
);