namespace AgendAi.API.Contracts.Profissionais;

public sealed record ListarProcedimentosProfissionalResponse(
    IReadOnlyCollection<ProcedimentoProfissionalResponse> Procedimentos
);