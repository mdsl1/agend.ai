namespace AgendAi.Infrastructure.Auth;

using AgendAi.Application.Auth.Models;
using AgendAi.Application.Auth.Ports;
using AgendAi.Domain.Profissionais;
using AgendAi.Domain.Usuarios;
using NHibernate;
using NHibernate.Linq;

public sealed class AutenticacaoUsuarioReader : IAutenticacaoUsuarioReader
{
    private readonly ISession _session;

    public AutenticacaoUsuarioReader(ISession session)
    {
        _session = session;
    }

    public async Task<DadosAutenticacaoUsuario?> BuscarPorClinicaEEmailAsync(
        Guid clinicaUuid,
        string email,
        CancellationToken cancellationToken
    )
    {
        var emailInput = email.Trim().ToLowerInvariant();

        var usuario = await _session
            .Query<Usuario>()
            .Where(usuario =>
                usuario.Clinica.Uuid == clinicaUuid
                && usuario.Clinica.DeletedAt == null
                && usuario.DeletedAt == null
                && usuario.Email.Trim().ToLower() == emailInput
            )
            .Select(usuario => new
            {
                UsuarioId = usuario.Id,
                UsuarioUuid = usuario.Uuid,
                ClinicaUuid = usuario.Clinica.Uuid,
                usuario.Nome,
                usuario.Email,
                usuario.SenhaHash,
                usuario.Cargo,
                usuario.IsAdmin
            }).SingleOrDefaultAsync(cancellationToken);

        if (usuario is null)
        {
            return null;
        }

        var profissionalUuid = await _session
            .Query<Profissional>()
            .Where(profissional =>
                profissional.Usuario.Id == usuario.UsuarioId
                && profissional.Clinica.Uuid == usuario.ClinicaUuid
                && profissional.DeletedAt == null
            )
            .Select(profissional => (Guid?)profissional.Uuid)
            .SingleOrDefaultAsync(cancellationToken);

        return new DadosAutenticacaoUsuario(
            usuario.UsuarioUuid,
            usuario.ClinicaUuid,
            usuario.Nome,
            usuario.Email,
            usuario.SenhaHash,
            usuario.Cargo,
            usuario.IsAdmin,
            profissionalUuid
        );
    }
}