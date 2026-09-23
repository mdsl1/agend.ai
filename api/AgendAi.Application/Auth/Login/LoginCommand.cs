namespace AgendAi.Application.Auth.Login;

public sealed record LoginCommand(
    Guid ClinicaUuid,
    string Email,
    string Senha
);