namespace AgendAi.Infrastructure.Profissionais;

using AgendAi.Application.Profissionais.ListarProcedimentosProfissional.Models;
using AgendAi.Application.Profissionais.ListarProcedimentosProfissional.Ports;
using AgendAi.Domain.Profissionais;
using NHibernate;
using NHibernate.Linq;

public sealed class ProcedimentosProfissionalReader : IProcedimentosProfissionalReader
{
    private readonly ISession _session;

    public ProcedimentosProfissionalReader(ISession session)
    {
        _session = session;
    }

    public async Task<DadosListagemProcedimentosProfissional?> ListarAsync(
        Guid profissionalUuid,
        CancellationToken cancellationToken
    )
    {
        var profissionalEncontrado = await _session
            .Query<Profissional>()
            .Where(profissional =>
                profissional.Uuid == profissionalUuid
                && profissional.DeletedAt == null
                && profissional.Clinica.DeletedAt == null
                && profissional.Usuario.DeletedAt == null    
            )
            .Select(profissional => new
            {
                ProfissionalUuid = profissional.Uuid,
                ClinicaUuid = profissional.Clinica.Uuid
            }).SingleOrDefaultAsync(cancellationToken);

        if (profissionalEncontrado is null)
        {
            return null;
        }

        var procedimentos = await _session
            .Query<ProfissionalProcedimento>()
            .Where(vinculo => 
                vinculo.Profissional.Uuid == profissionalUuid
                && vinculo.Profissional.DeletedAt == null
                && vinculo.DeletedAt == null
                && vinculo.Procedimento.DeletedAt == null
                && vinculo.Clinica.DeletedAt == null
                && vinculo.Profissional.Usuario.DeletedAt == null
            )
            .OrderBy(vinculo => vinculo.Procedimento.Nome)
            .Select(vinculo => new DadosProcedimentoProfissional(
                vinculo.Uuid,
                vinculo.Procedimento.Uuid,
                vinculo.Procedimento.Nome,
                vinculo.Valor,
                vinculo.DuracaoMinutos
            )).ToListAsync(cancellationToken);

        return new DadosListagemProcedimentosProfissional(
            ProfissionalUuid: profissionalEncontrado.ProfissionalUuid,
            ClinicaUuid: profissionalEncontrado.ClinicaUuid,
            Procedimentos: procedimentos
        );
    }
}
