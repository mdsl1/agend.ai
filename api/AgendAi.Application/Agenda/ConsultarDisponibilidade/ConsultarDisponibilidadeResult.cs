namespace AgendAi.Application.Agenda.ConsultarDisponibilidade;

public sealed record ConsultarDisponibilidadeResult (
    bool Disponivel,
    int DuracaoMinutos,
    IReadOnlyCollection<HorarioDisponivelResult> HorariosDisponiveis,
    bool BuscaEsgotada
);