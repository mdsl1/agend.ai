namespace AgendAi.Application.Agenda.Models;

public sealed record ResultadoDisponibilidadeExterna (
    bool Disponivel,
    IReadOnlyCollection<HorarioDisponivelExterno> HorariosDisponiveis,
    bool BuscaEsgotada
);