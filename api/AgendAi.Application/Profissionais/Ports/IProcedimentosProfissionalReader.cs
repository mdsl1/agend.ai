namespace AgendAi.Application.Profissionais.Ports;

using AgendAi.Application.Profissionais.Models;

public interface IProcedimentosProfissionalReader
{
    Task<DadosListagemProcedimentosProfissional?> ListarAsync(
        Guid profissionalUuid,
        CancellationToken cancellationToken
    );
}