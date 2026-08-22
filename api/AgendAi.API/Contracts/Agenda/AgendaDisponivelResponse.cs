namespace AgendAi.API.Contracts.Agenda;

public sealed record AgendaDisponivelResponse (
    Guid ProfissionalUuid,
    string NomeExibicao
);