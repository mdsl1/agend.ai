namespace AgendAi.Infrastructure.Agenda;

using AgendAi.Application.Agenda.Ports;
using AgendAi.Application.Common.Exceptions;
using Domain.Agendamentos;
using NHibernate;
using Npgsql;

public sealed class AgendamentoWriter : IAgendamentoWriter
{
    private const string ConstraintSobreposicao = "ex_agendamento_sem_sobreposicao";
    private const string ConstraintIdempotencia = "uq_agendamento_clinica_chave_idempotencia";

    private readonly ISession _session;

    public AgendamentoWriter(ISession session)
    {
        _session = session;
    }

    public async Task InserirAsync(
        Agendamento agendamento,
        CancellationToken cancellationToken
    )
    {
        ArgumentNullException.ThrowIfNull(agendamento);

        using var transaction = _session.BeginTransaction();

        try
        {
            await _session.SaveAsync(
                agendamento,
                cancellationToken
            );

            await transaction.CommitAsync(cancellationToken);
        }
        catch (Exception exception)
        {
            await DoRollbackAsync(transaction);

            if(IsViolacaoConstraint(exception, ConstraintIdempotencia))
            {
                throw new ConflitoException(
                    codigo: "idempotency_key_em_uso",
                    mensagem: "Já existe uma operação registrada com essa chave. Reenvie a mesma requisição com a mesma Idempotency-Key.",
                    innerException: exception
                );
            }

            if(IsViolacaoConstraint(exception, ConstraintSobreposicao))
            {
                throw new ConflitoException(
                    codigo: "horario_indisponivel",
                    mensagem: "O horário solicitado não está mais disponível.",
                    innerException: exception
                );
            }

            throw;
        }
    }

    public async Task AtualizarAsync(
        Agendamento agendamento,
        CancellationToken cancellationToken
    )
    {
        ArgumentNullException.ThrowIfNull(agendamento);

        using var transaction = _session.BeginTransaction();

        try
        {
            await _session.SaveOrUpdateAsync(
                agendamento,
                cancellationToken
            );

            await transaction.CommitAsync(cancellationToken);
        }
        catch (Exception exception)
        {
            await DoRollbackAsync(transaction);

            if(IsViolacaoConstraint(exception, ConstraintSobreposicao))
            {
                throw new ConflitoException(
                    codigo: "horario_indisponivel",
                    mensagem: "O horário solicitado não está mais disponível.",
                    innerException: exception
                );
            }

            throw;
        }
    }

    private static async Task DoRollbackAsync(ITransaction transaction)
    {
        if(transaction.IsActive)
        {
            await transaction.RollbackAsync(CancellationToken.None);
        }
    }

    private static bool IsViolacaoConstraint(
        Exception exception,
        string nomeConstraint
    )
    {
        Exception? exceptionAtual = exception;

        while (exceptionAtual is not null)
        {
            if (
                exceptionAtual is PostgresException postgresException
                && postgresException.ConstraintName == nomeConstraint
            )
            {
                return true;
            }

            exceptionAtual = exceptionAtual.InnerException;
        }

        return false;
    }
}