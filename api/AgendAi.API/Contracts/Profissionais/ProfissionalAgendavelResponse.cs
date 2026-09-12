namespace AgendAi.API.Contracts.Profissionais;

public sealed record ProfissionalAgendavelResponse(
    Guid ProfissionalUuid,
    string NomeExibicao,
    EspecialidadeProfissionalResponse? Especialidade
);