namespace AgendAi.API.Contracts.Auth;

public sealed record LoginRequest(
    Guid ClinicaUuid,
    string Email,
    string Senha
);