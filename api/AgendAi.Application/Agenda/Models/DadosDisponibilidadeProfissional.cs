namespace AgendAi.Application.Agenda.Models;

public sealed record DadosDisponibilidadeProfissional (
    Guid ProfissionalUuid,
    Guid ProfissionalProcedimentoUuid,
    Guid ClinicaUuid,
    int DuracaoMinutos,
    string? IdAgendaExterna,
    string? UrlWebhookAgenda,
    IReadOnlyCollection<DadosJanelaAtendimento> JanelasAtendimento
);