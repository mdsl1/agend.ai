namespace AgendAi.Application.Agenda.Models;

using AgendAi.Domain.Clientes;
using AgendAi.Domain.Profissionais;

public sealed record DadosCriacaoAgendamento(
    Cliente Cliente,
    ProfissionalProcedimento ProfissionalProcedimento,
    IReadOnlyCollection<DadosJanelaAtendimento> JanelasAtendimento
);