namespace AgendAi.Application.Agenda.ConsultarDisponibilidade;

public sealed record ConsultarDisponibilidadeQuery (
    Guid ProfissionalUuid,
    Guid ProfissionalProcedimentoUuid,
    DateTimeOffset Inicio,
    int Limite
);