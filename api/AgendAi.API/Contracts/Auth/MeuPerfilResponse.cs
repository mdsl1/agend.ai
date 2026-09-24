namespace AgendAi.API.Contracts.Auth;

public sealed record MeuPerfilResponse(
    Guid UsuarioUuid,
    Guid? ProfissionalUuid,
    string Nome,
    string? Prefixo,
    string Email,
    string Cargo,
    bool IsAdmin,
    string? RegistroProfissional,
    string? Especialidade,
    ClinicaUsuarioResponse Clinica,
    IReadOnlyCollection<string> Permissoes
);