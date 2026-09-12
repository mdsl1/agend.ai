namespace AgendAi.Application.Profissionais.ListarProfissionais;

public sealed record ListarProfissionaisQuery(
    Guid ClinicaUuid,
    Guid? EspecialidadeUuid
);