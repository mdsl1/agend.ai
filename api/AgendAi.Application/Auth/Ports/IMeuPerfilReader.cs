namespace AgendAi.Application.Auth.Ports;

using AgendAi.Application.Auth.Models;

public interface IMeuPerfilReader
{
    Task<DadosMeuPerfil?> ObterAsync(
        Guid usuarioUuid,
        Guid clinicaUuid,
        CancellationToken cancellationToken
    );
}