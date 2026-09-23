namespace AgendAi.API.Contracts.Auth;

public sealed record LoginResponse(
    string AccessToken,
    DateTimeOffset ExpiraEm,
    UsuarioAutenticadoResponse Usuario
);