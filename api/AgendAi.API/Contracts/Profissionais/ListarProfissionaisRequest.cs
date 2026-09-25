namespace AgendAi.API.Contracts.Profissionais;

public sealed record ListarProfissionaisRequest {
    public Guid? EspecialidadeUuid { get; init; }
}