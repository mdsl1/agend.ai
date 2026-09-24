namespace AgendAi.API.Contracts.Auth;

public sealed record ClinicaUsuarioResponse(
    Guid Uuid,
    string Nome
);