namespace AgendAi.API.Contracts.Profissionais;

public sealed record AlterarMeuProcedimentoRequest(
    int? DuracaoMinutos,
    decimal? Valor,
    bool? DisponivelAgendamento
);