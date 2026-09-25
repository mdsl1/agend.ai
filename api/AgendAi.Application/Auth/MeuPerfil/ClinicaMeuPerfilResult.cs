namespace AgendAi.Application.Auth.MeuPerfil;

public sealed record ClinicaMeuPerfilResult(
    Guid Uuid,
    string Nome,
    string TipoClinica
);