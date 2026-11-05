namespace AgendAi.Infrastructure.Agenda;

using AgendAi.Domain.Agendamentos;
using AgendAi.Application.Agenda.Ports;
using AgendAi.Application.Agenda.Models;
using NHibernate;
using NHibernate.Linq;

public sealed class AgendamentoStatusWriter : IAgendamentoStatusWriter
{
    private readonly ISession _session;

    public AgendamentoStatusWriter(
        ISession session
    )
    {
        _session = session;
    }

    public async Task<ResultadoAlteracaoStatusAgendamento?> AlterarAsync(
        Guid agendamentoUuid,
        Guid clinicaUuid,
        string novoStatus,
        DateTimeOffset alteradoEm,
        CancellationToken cancellationToken
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(novoStatus);

        using var transaction = _session.BeginTransaction();

        var agendamento = await _session
            .Query<Agendamento>()
            .Where(agendamento =>
                agendamento.Uuid == agendamentoUuid
                && agendamento.Clinica.Uuid == clinicaUuid
                && agendamento.Clinica.DeletedAt == null
                && agendamento.DeletedAt == null
            ).SingleOrDefaultAsync(cancellationToken);

        if (agendamento is null )
        {
            await transaction.RollbackAsync(cancellationToken);
            return null;
        }

        var statusAnterior = agendamento.Status;
        var statusPgtoAnterior = agendamento.StatusPagamento;

        try {
            switch (novoStatus.ToLower().Trim())
            {
                case Agendamento.StatusConcluido:
                    agendamento.MarcarComoConcluido(alteradoEm);
                    break;
                case Agendamento.StatusConfirmado:
                    agendamento.MarcarComoConfirmado(alteradoEm);
                    break;
                case Agendamento.StatusFaltou:
                    agendamento.MarcarComoFaltou(alteradoEm);
                    break;
                case Agendamento.StatusCancelado:
                    agendamento.MarcarComoCancelado(alteradoEm);
                    break;
                default:
                    throw new ArgumentException(
                        $"A alteração para o status {novoStatus.ToLower().Trim()} não é suportada.",
                        nameof(novoStatus)
                    );
            }

            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            if (transaction.IsActive)
            {
                await transaction.RollbackAsync(CancellationToken.None);
            }

            throw;
        }

        return new ResultadoAlteracaoStatusAgendamento(
            AgendamentoUuid: agendamento.Uuid,
            StatusAnterior: statusAnterior,
            StatusPgtoAnterior: statusPgtoAnterior,
            NovoStatus: agendamento.Status,
            NovoStatusPgto: agendamento.StatusPagamento,
            AlteradoEm: agendamento.UpdatedAt ?? alteradoEm
        );

    }
}