namespace AgendAi.Application.Profissionais.AlterarMeuProcedimento;

public sealed record AlterarMeuProcedimentoCommand(
    Guid ProfissionalProcedimentoUuid,
    int? DuracaoMinutos,
    decimal? Valor,
    bool? DisponivelAgendamento
);