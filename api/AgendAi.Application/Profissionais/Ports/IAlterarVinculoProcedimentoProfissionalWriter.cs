namespace AgendAi.Application.Profissionais.Ports;

using AgendAi.Application.Profissionais.Models;

public interface IAlterarVinculoProcedimentoProfissionalWriter
{
    Task<bool> AlterarAsync(
        DadosAlteracaoVinculoProcedimentoProfissional dados,
        CancellationToken cancellationToken
    );
}