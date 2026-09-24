namespace AgendAi.Application.Auth.Models;

public sealed record DadosMeuPerfil(
    Guid UsuarioUuid,
    Guid ClinicaUuid,
    string ClinicaNome,
    string Nome,
    string Email,
    string Cargo,
    bool IsAdmin,
    Guid? ProfissionalUuid,
    string? Prefixo,
    string? RegistroProfissional,
    string? Especialidade
);