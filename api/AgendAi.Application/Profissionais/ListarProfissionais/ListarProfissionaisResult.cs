namespace AgendAi.Application.Profissionais.ListarProfissionais;

public sealed record ListarProfissionaisResult(
    IReadOnlyCollection<ProfissionalAgendavelResult> Profissionais
);