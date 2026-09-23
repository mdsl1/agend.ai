namespace AgendAi.Application.Auth.Models;

public sealed record DadosGeracaoAccessToken(
    Guid UsuarioUuid,
    Guid ClinicaUuid,
    string Cargo,
    bool IsAdmin,
    Guid? ProfissionalUuid
);