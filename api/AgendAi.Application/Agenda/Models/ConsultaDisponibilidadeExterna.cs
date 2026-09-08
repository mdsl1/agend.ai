namespace AgendAi.Application.Agenda.Models;

public sealed record ConsultaDisponibilidadeExterna (
    Guid ProfissionalUuid,
    string IdAgendaExterna,
    string Timezone,
    DateTimeOffset Inicio,
    DateTimeOffset Fim,
    int QuantidadeAlternativas,
    int DiasBusca,
    int DuracaoMinutos,
    IReadOnlyCollection<DadosJanelaAtendimento> JanelasAtendimento
);