namespace AgendAi.API.Contracts.Disponibilidade;

public sealed record ConsultarDisponibilidadeResponse(
    bool Disponivel,
    int DuracaoMinutos,
    IReadOnlyCollection<HorarioDisponivelResponse> HorariosDisponiveis,
    bool BuscaEsgotada
);