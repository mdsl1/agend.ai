namespace AgendAi.API.Contracts.Auth;

public sealed record UsuarioAutenticadoResponse(
    Guid Uuid,
    string Nome,
    string Cargo,
    bool IsAdmin,
    Guid? ProfissionalUuid,
    Guid ClinicaUuid
);