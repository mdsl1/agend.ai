namespace AgendAi.Infrastructure.Auth;

using AgendAi.Application.Auth.Models;
using AgendAi.Application.Auth.Ports;
using AgendAi.Domain.Profissionais;
using AgendAi.Domain.Usuarios;
using NHibernate;
using NHibernate.Linq;

public class MeuPerfilReader : IMeuPerfilReader
{
    private readonly ISession _session;
    public MeuPerfilReader(ISession session)
    {
        _session = session;
    }

    public async Task<DadosMeuPerfil?> ObterAsync(
        Guid usuarioUuid,
        Guid clinicaUuid,
        CancellationToken cancellationToken
    )
    {
        var usuario = await _session
            .Query<Usuario>()
            .Where(usuario => 
                usuario.Uuid == usuarioUuid
                && usuario.Clinica.Uuid == clinicaUuid
                && usuario.DeletedAt == null
                && usuario.Clinica.DeletedAt == null
            )
            .Select(usuario => new
            {
                UsuarioUuid = usuario.Uuid,
                ClinicaUuid = usuario.Clinica.Uuid,
                ClinicaNome = usuario.Clinica.Nome,
                usuario.Nome,
                usuario.Email,
                usuario.Cargo,
                usuario.IsAdmin
            }).SingleOrDefaultAsync(cancellationToken);
        
        if (usuario is null)
        {
            return null;
        }

        var profissional = await _session
            .Query<Profissional>()
            .Where(profissional => 
                profissional.Usuario.Uuid == usuarioUuid
                && profissional.Clinica.Uuid == clinicaUuid
                && profissional.DeletedAt == null
                && profissional.Usuario.DeletedAt == null
                && profissional.Clinica.DeletedAt == null
            )
            .Select(profissional => new
            {
                profissional.Uuid,
                profissional.Prefixo,
                profissional.RegistroProfissional,
                Especialidade = profissional.Especialidade != null && profissional.Especialidade.DeletedAt == null
                    ? profissional.Especialidade.Nome
                    : null
            }).SingleOrDefaultAsync(cancellationToken);

            return new DadosMeuPerfil(
                usuario.UsuarioUuid,
                usuario.ClinicaUuid,
                usuario.ClinicaNome,
                usuario.Nome,
                usuario.Email,
                usuario.Cargo,
                usuario.IsAdmin,
                profissional?.Uuid,
                profissional?.Prefixo,
                profissional?.RegistroProfissional,
                profissional?.Especialidade
            );
    }
}