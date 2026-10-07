namespace AgendAi.Application.Profissionais.ListarMeusProcedimentos;

public sealed record ListarMeusProcedimentosResult(
    IReadOnlyCollection<MeuProcedimentoResult> Procedimentos
);