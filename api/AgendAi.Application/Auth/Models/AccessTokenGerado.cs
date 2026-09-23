namespace AgendAi.Application.Auth.Models;

public sealed record AccessTokenGerado(
    string JWT,
    DateTimeOffset ExpiraEm
);