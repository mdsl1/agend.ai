namespace AgendAi.API.Contracts.Agendamentos;

public sealed record CriarAgendamentoRequest
{
    public Guid ClienteUuid { get; init; }
    public Guid ProfissionalProcedimentoUuid { get; init; }
    public DateTimeOffset Inicio { get; init;}
    public string? MotivoContato { get; init; }

}