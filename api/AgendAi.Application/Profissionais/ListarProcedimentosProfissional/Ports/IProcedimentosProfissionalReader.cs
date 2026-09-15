namespace AgendAi.Application.Profissionais.ListarProcedimentosProfissional.Ports;

using AgendAi.Application.Profissionais.ListarProcedimentosProfissional.Models;

public interface IProcedimentosProfissionalReader
{
    Task<DadosListagemProcedimentosProfissional?> ListarAsync(
        Guid profissionalUuid,
        CancellationToken cancellationToken
    );
}