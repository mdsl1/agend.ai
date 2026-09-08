namespace AgendAi.API.Contracts.Disponibilidade;

public sealed record HorarioDisponivelResponse (
    DateTimeOffset Inicio,
    DateTimeOffset Fim
);