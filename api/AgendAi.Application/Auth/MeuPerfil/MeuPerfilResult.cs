namespace AgendAi.Application.Auth.MeuPerfil;

public sealed record MeuPerfilResult(
    Guid UsuarioUuid,
    Guid? ProfissionalUuid,
    string Nome,
    string? Prefixo,
    string Email,
    string Cargo,
    bool IsAdmin,
    string? RegistroProfissional,
    string? Especialidade,
    ClinicaMeuPerfilResult Clinica,
    IReadOnlyCollection<string> Permissoes
);