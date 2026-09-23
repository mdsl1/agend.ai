namespace AgendAi.Application.Auth.Login;

public sealed record UsuarioAutenticadoResult(
    Guid Uuid,
    string Nome,
    string Cargo,
    bool IsAdmin,
    Guid? ProfissionalUuid,
    Guid ClinicaUuid
);