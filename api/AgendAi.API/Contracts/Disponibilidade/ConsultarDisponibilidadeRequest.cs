namespace AgendAi.API.Contracts.Disponibilidade;

public sealed record ConsultarDisponibilidadeRequest
{
    public Guid ProfissionalProcedimentoUuid { get; init; }
    public DateTimeOffset Inicio { get; init; }
    public int Limite { get; init; } = 3;
}