using AgendAi.Application.Auth.Models;

namespace AgendAi.Application.Auth.Ports;

public interface IAutenticacaoUsuarioReader
{
    Task<DadosAutenticacaoUsuario?> BuscarPorClinicaEEmailAsync(
        Guid clinicaUuid,
        string email,
        CancellationToken cancellationToken
    );
}