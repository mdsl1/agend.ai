namespace AgendAi.Application.Profissionais.ListarProcedimentosProfissional.Models;

public sealed record DadosListagemProcedimentosProfissional(
    Guid ProfissionalUuid,
    IReadOnlyCollection<DadosProcedimentoProfissional> Procedimentos
);