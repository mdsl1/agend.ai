using AgendAi.Application.Agenda.Models;
using AgendAi.Domain.Agendamentos;

namespace AgendAi.Application.Agenda.Ports;

public interface ICriacaoAgendamentoReader
{
    Task<DadosCriacaoAgendamento?> ObterAsync(
        Guid clienteUuid,
        Guid profissionalProcedimentoUuid,
        CancellationToken cancellationToken
    );

    Task<Agendamento?> ObterPorChaveIdempotenciaAsync(
        Guid clinicaUuid,
        string chaveIdempotencia,
        CancellationToken cancellationToken
    );
}