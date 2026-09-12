namespace AgendAi.Application.Agenda.Models;

public sealed record ResultadoCriacaoAgendamentoExterno(
    string IdEventoExterno,
    Guid AgendamentoUuid,
    DateTimeOffset Inicio,
    DateTimeOffset Fim
);