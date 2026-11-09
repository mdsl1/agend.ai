namespace AgendAi.Application.Profissionais.Ports;

using AgendAi.Application.Profissionais.Models;

public interface IVinculoProcedimentoProfissionalReader
{
    Task<DadosVinculoProcedimentoProfissional?> ConsultarAsync(
        Guid clinicaUuid,
        Guid profissionalUuid,
        Guid procedimentoUuid,
        CancellationToken cancellationToken
    );

    Task<DadosVinculoProcedimentoProfissional?> ConsultarPorUuidAsync(
        Guid clinicaUuid,
        Guid profissionalUuid,
        Guid profissionalProcedimentoUuid,
        CancellationToken cancellationToken
    );
}