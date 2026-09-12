namespace AgendAi.API.Contracts.Profissionais;

public sealed record ListarProfissionaisResponse(
    IReadOnlyCollection<ProfissionalAgendavelResponse> Profissionais
);