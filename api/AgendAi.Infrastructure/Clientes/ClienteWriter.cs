namespace AgendAi.Infrastructure.Clientes;

using AgendAi.Application.Clientes.Ports;
using AgendAi.Application.Common.Exceptions;
using AgendAi.Domain.Clientes;
using NHibernate;
using Npgsql;

public sealed class ClienteWriter : IClienteWriter
{
    private const string ConstraintTelefone = "uq_cliente_telefone_ativo";
    private const string ConstraintTelegram = "uq_cliente_telegram_ativo";

    private readonly ISession _session;

    public ClienteWriter(ISession session)
    {
        _session = session;
    }

    public Task InserirAsync(
        Cliente cliente,
        CancellationToken cancellationToken
    )
    {
        return PersistirAsync(
            cliente,
            inserir: true,
            cancellationToken
        );
    }

    public Task AtualizarAsync(
        Cliente cliente,
        CancellationToken cancellationToken
    )
    {
        return PersistirAsync(
            cliente,
            inserir: false,
            cancellationToken
        );
    }

    private async Task PersistirAsync(
        Cliente cliente,
        bool inserir,
        CancellationToken cancellationToken
    )
    {
        ArgumentNullException.ThrowIfNull(cliente);

        using var transaction = _session.BeginTransaction();

        try
        {
            if (inserir)
            {
                await _session.SaveAsync(
                    cliente,
                    cancellationToken
                );
            }
            else
            {
                await _session.SaveOrUpdateAsync(
                    cliente,
                    cancellationToken
                );
            }

            await transaction.CommitAsync(cancellationToken);
        }
        catch(Exception exception)
        {
            await DoRollbackAsync(transaction);

            if (IsViolacaoConstraint(
                exception,
                ConstraintTelefone
            ))
            {
                throw new ConflitoException(
                    codigo: "cliente_telefone_em_uso",
                    mensagem: "Já existe um cliente ativo com esse telefone.",
                    innerException: exception
                );
            }

            if (IsViolacaoConstraint(
                exception,
                ConstraintTelegram
            ))
            {
                throw new ConflitoException(
                    codigo: "telegram_user_id_em_uso",
                    mensagem: "Esse usuário do Telegram já está vinculado a outro cliente.",
                    innerException: exception
                );
            }

            throw;
        }
    }

    private static async Task DoRollbackAsync(ITransaction transaction)
    {
        if (transaction.IsActive)
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