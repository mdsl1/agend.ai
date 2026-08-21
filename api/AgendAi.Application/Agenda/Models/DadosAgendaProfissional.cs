namespace AgendAi.Application.Agenda.Models;

public sealed record DadosAgendaProfissional (
    Guid ProfissionalUuid,
    Guid ClinicaUuid,
    string? IdAgendaExterna,
    string? UrlWebhookAgenda
);