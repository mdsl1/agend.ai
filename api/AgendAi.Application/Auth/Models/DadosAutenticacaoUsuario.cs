namespace AgendAi.Application.Auth.Models;

public sealed record DadosAutenticacaoUsuario(
    Guid UsuarioUuid,
    Guid ClinicaUuid,
    string Nome,
    string Email,
    string SenhaHash,
    string Cargo,
    bool IsAdmin,
    Guid? ProfissionalUuid
);