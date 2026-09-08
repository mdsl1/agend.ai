namespace AgendAi.Application.Agenda.Ports;

using AgendAi.Application.Agenda.Models;

public interface IDisponibilidadeReader
{
    Task <DadosDisponibilidadeProfissional?> ObterAsync(
        Guid profissionalUuid,
        Guid profissionalProcedimentoUuid,
        CancellationToken cancellationToken
    );
}