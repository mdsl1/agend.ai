namespace AgendAi.Application.Agenda.Models;

public sealed record DadosJanelaAtendimento (
    short DiaSemana,
    TimeSpan HoraInicio,
    TimeSpan HoraFim
);