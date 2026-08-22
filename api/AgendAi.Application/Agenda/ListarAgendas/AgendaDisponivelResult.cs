namespace AgendAi.Application.Agenda.ListarAgendas;

public sealed record AgendaDisponivelResult (
    Guid ProfissionalUuid,
    string NomeExibicao
);