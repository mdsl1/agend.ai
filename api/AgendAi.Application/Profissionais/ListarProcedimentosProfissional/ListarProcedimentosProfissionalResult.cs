namespace AgendAi.Application.Profissionais.ListarProcedimentosProfissional;

public sealed record ListarProcedimentosProfissionalResult(
    IReadOnlyCollection<ProcedimentoProfissionalResult> Procedimentos
);