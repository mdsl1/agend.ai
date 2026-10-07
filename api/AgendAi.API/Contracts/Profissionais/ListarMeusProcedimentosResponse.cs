namespace AgendAi.API.Contracts.Profissionais;

public sealed record ListarMeusProcedimentosResponse(
    IReadOnlyCollection<ProcedimentoProfissionalResponse> Procedimentos
);