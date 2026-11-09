namespace AgendAi.Infrastructure.Profissionais;

using AgendAi.Application.Profissionais.Models;
using AgendAi.Application.Profissionais.Ports;
using AgendAi.Domain.Profissionais;
using NHibernate;
using NHibernate.Linq;

public sealed class AlterarVinculoProcedimentoProfissionalWriter : IAlterarVinculoProcedimentoProfissionalWriter
{
    private readonly ISession _session;

    public AlterarVinculoProcedimentoProfissionalWriter(ISession session)
    {
        _session = session;
    }

    public async Task<bool> AlterarAsync(
        DadosAlteracaoVinculoProcedimentoProfissional dados,
        CancellationToken cancellationToken
    )
    {
        ArgumentNullException.ThrowIfNull(dados);

        if (
            dados.ClinicaUuid == Guid.Empty
            || dados.ProfissionalProcedimentoUuid == Guid.Empty
        )
        {
            throw new ArgumentException(
                "Clínica e vinculo procedimento-profissional devem ter UUIDs válidos.",
                nameof(dados)
            );
        }

        using var transaction = _session.BeginTransaction();

        try
        {
            var vinculo = await _session
                .Query<ProfissionalProcedimento>()
                .Where(vinculo =>
                    vinculo.Clinica.Uuid == dados.ClinicaUuid
                    && vinculo.Uuid == dados.ProfissionalProcedimentoUuid
                    && vinculo.Clinica.DeletedAt == null
                    && vinculo.Profissional.DeletedAt == null
                    && vinculo.Profissional.Usuario.DeletedAt == null
                )
                .SingleOrDefaultAsync(cancellationToken);
            
            if (vinculo is null)
            {
                await transaction.RollbackAsync(cancellationToken);
                return false;
            }

            vinculo.AlterarVinculo(
                duracaoMinutos: dados.DuracaoMinutos,
                valor: dados.Valor,
                deletedAt: dados.DeletedAt
            );

            await transaction.CommitAsync(cancellationToken);
            return true;
        }
        catch
        {
            if (transaction.IsActive)
            {
                await transaction.RollbackAsync(CancellationToken.None);
            }

            throw;
        }
    }
}