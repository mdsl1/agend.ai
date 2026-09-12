namespace AgendAi.API.Contracts.Profissionais;

public sealed record ListarProfissionaisRequest {
    public Guid ClinicaUuid { get; init; }
    public Guid? EspecialidadeUuid { get; init; }
}