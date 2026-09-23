namespace AgendAi.Application.Auth.Login;

public sealed record LoginResult(
    string AccessToken,
    DateTimeOffset ExpiraEm,
    UsuarioAutenticadoResult Usuario
);