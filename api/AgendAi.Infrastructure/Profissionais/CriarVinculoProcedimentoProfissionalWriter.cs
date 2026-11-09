namespace AgendAi.Infrastructure.Profissionais;

using AgendAi.Application.Common.Exceptions;
using AgendAi.Application.Profissionais.Models;
using AgendAi.Application.Profissionais.Ports;
using AgendAi.Domain.Profissionais;
using NHibernate;
using NHibernate.Linq;
using Npgsql;

public sealed class CriarVinculoProcedimentoProfissionalWriter : ICriarVinculoProcedimentoProfissionalWriter
{
    private const string ConstraintVinculoUnico = "uq_profissional_procedimento";
    private readonly ISession _session;

    public CriarVinculoProcedimentoProfissionalWriter( ISession session)
    {
        _session = session;
    }

    public async Task<Guid> InserirAsync(
        DadosCriacaoVinculoProcedimentoProfissional dados,
        CancellationToken cancellationToken
    )
    {
        ArgumentNullException.ThrowIfNull(dados);

        if (
            dados.ClinicaUuid == Guid.Empty
            || dados.ProfissionalUuid == Guid.Empty
            || dados.ProcedimentoUuid == Guid.Empty
        )
        {
            throw new ArgumentException(
                "Clínica, Profissional e Procedimento devem ter UUIDs válidos.",
                nameof(dados)
            );
        }

        using var transaction = _session.BeginTransaction();

        try
        {
            var profissional = await _session
                .Query<Profissional>()
                .Where(profissional =>
                    profissional.Uuid == dados.ProfissionalUuid
                    && profissional.Clinica.Uuid == dados.ClinicaUuid
                    && profissional.DeletedAt == null
                    && profissional.Clinica.DeletedAt == null
                    && profissional.Usuario.DeletedAt == null
                )
                .SingleOrDefaultAsync(cancellationToken);

            if (profissional is null)
            {
                throw new InvalidOperationException("O profissional não foi encontrado na clínica informada.");
            }

            var procedimento = await _session
                .Query<Procedimento>()
                .Where(procedimento =>
                    procedimento.Uuid == dados.ProcedimentoUuid
                    && procedimento.Clinica.Uuid == dados.ClinicaUuid
                    && procedimento.DeletedAt == null
                    && procedimento.Clinica.DeletedAt == null
                )
                .SingleOrDefaultAsync(cancellationToken);
            
            if (procedimento is null)
            {
                throw new InvalidOperationException("O procedimento não foi encontrado na clínica informada.");
            }

            var vinculo = ProfissionalProcedimento.CriarVinculo(
                profissional,
                procedimento,
                dados.DuracaoMinutos,
                dados.Valor
            );

            await _session.SaveAsync(vinculo, cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return vinculo.Uuid;
        }
        catch(Exception exception)
        {
            if (transaction.IsActive)
            {
                await transaction.RollbackAsync(CancellationToken.None);
            }

            if (IsViolacaoDoVinculoUnico(exception))
            {
                throw new ConflitoException(
                    "procedimento_ja_vinculado",
                    "Esse procedimento já possui um vinculo com o profissional.",
                    innerException: exception
                );
            }

            throw;
        }
    }

    private static bool IsViolacaoDoVinculoUnico(Exception exception)
    {
        Exception? atual = exception;

        while (atual is not null)
        {
            if(
                atual is PostgresException postgresException
                && postgresException.ConstraintName == ConstraintVinculoUnico
            )
            {
                return true;
            }

            atual = atual.InnerException;

        }

        return false;
    }
}