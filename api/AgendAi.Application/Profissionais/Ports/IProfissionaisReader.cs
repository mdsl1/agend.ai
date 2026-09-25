namespace AgendAi.Application.Profissionais.Ports;

using AgendAi.Application.Profissionais.Models;

public interface IProfissionaisReader
{
    Task<IReadOnlyCollection<DadosProfissionalAgendavel>> ListarAsync(
        Guid clinicaUuid,
        Guid? especialidadeUuid,
        Guid? profissionalUuid,
        CancellationToken cancellationToken
    );
}