namespace AgendAi.Infrastructure.Profissionais;

using AgendAi.Application.Profissionais.Models;
using AgendAi.Application.Profissionais.Ports;
using AgendAi.Domain.Profissionais;
using NHibernate;
using NHibernate.Linq;

public sealed class VinculoProcedimentoProfissionalReader : IVinculoProcedimentoProfissionalReader
{
    private readonly ISession _session;

    public VinculoProcedimentoProfissionalReader(ISession session)
    {
        _session = session;
    }

    public async Task<DadosVinculoProcedimentoProfissional?> ConsultarAsync(
        Guid clinicaUuid,
        Guid profissionalUuid,
        Guid procedimentoUuid,
        CancellationToken cancellationToken
    )
    {
        var procedimento = await _session
            .Query<Procedimento>()
            .Where(procedimento =>
                procedimento.Clinica.Uuid == clinicaUuid
                && procedimento.Uuid == procedimentoUuid
                && procedimento.DeletedAt == null
            ).Select(procedimento => new
            {
                ProcedimentoUuid = procedimento.Uuid,
                procedimento.Nome,
                procedimento.ValorBase,
                procedimento.DuracaoEstimadaMinutos
            }).SingleOrDefaultAsync(cancellationToken);

        if (procedimento is null)
        {
            return null;
        }

        var vinculo = await _session
            .Query<ProfissionalProcedimento>()
            .Where(vinculo =>
                vinculo.Clinica.Uuid == clinicaUuid
                && vinculo.Profissional.Uuid == profissionalUuid
                && vinculo.Procedimento.Uuid == procedimentoUuid
                && vinculo.Clinica.DeletedAt == null
                && vinculo.Profissional.DeletedAt == null
                && vinculo.Procedimento.DeletedAt == null
            ).Select(vinculo => new
            {
                ProfissionalProcedimentoUuid = vinculo.Uuid,
                vinculo.DuracaoMinutos,
                vinculo.Valor,
                vinculo.DeletedAt
            }).SingleOrDefaultAsync(cancellationToken);
        
        return new DadosVinculoProcedimentoProfissional(
            ProcedimentoUuid: procedimento.ProcedimentoUuid,
            Nome: procedimento.Nome,
            DuracaoEstimadaMinutos: procedimento.DuracaoEstimadaMinutos,
            ValorBase: procedimento.ValorBase,
            ProfissionalProcedimentoUuid: vinculo?.ProfissionalProcedimentoUuid,
            DuracaoMinutos: vinculo?.DuracaoMinutos,
            Valor: vinculo?.Valor,
            DeletedAt: vinculo?.DeletedAt
        );
    }

    public async Task<DadosVinculoProcedimentoProfissional?> ConsultarPorUuidAsync(
        Guid clinicaUuid,
        Guid profissionalUuid,
        Guid profissionalProcedimentoUuid,
        CancellationToken cancellationToken
    )
    {
        return await _session
            .Query<ProfissionalProcedimento>()
            .Where(vinculo =>
                vinculo.Uuid == profissionalProcedimentoUuid
                && vinculo.Clinica.Uuid == clinicaUuid
                && vinculo.Profissional.Uuid == profissionalUuid
                && vinculo.Clinica.DeletedAt == null
                && vinculo.Profissional.DeletedAt == null
                && vinculo.Profissional.Usuario.DeletedAt == null
                && vinculo.Procedimento.DeletedAt == null
            )
            .Select(vinculo => new DadosVinculoProcedimentoProfissional(
                vinculo.Procedimento.Uuid,
                vinculo.Procedimento.Nome,
                vinculo.Procedimento.DuracaoEstimadaMinutos,
                vinculo.Procedimento.ValorBase,
                vinculo.Uuid,
                vinculo.DuracaoMinutos,
                vinculo.Valor,
                vinculo.DeletedAt
            )).SingleOrDefaultAsync(cancellationToken);
    }
}