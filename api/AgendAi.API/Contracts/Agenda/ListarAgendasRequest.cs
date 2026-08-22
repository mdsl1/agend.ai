namespace AgendAi.API.Contracts.Agenda;

public sealed record ListarAgendasRequest {
    public Guid ClinicaUuid { get; init; }
}
