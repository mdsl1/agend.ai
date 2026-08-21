namespace AgendAi.API.Contracts.Agenda;

public sealed record ConsultarAgendaRequest
{
    public DateTimeOffset Inicio { get; init; }
    public DateTimeOffset Fim { get; init; }
}