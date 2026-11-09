namespace AgendAi.Application.Profissionais.Ports;

using AgendAi.Application.Profissionais.Models;

public interface ICriarVinculoProcedimentoProfissionalWriter
{
    Task<Guid> InserirAsync(
        DadosCriacaoVinculoProcedimentoProfissional dados,
        CancellationToken cancellationToken
    );
}