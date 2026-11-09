namespace AgendAi.API.Contracts.Profissionais;

public sealed record VincularMeuProcedimentoResponse(
    Guid ProfissionalProcedimentoUuid,
    bool Criado,
    string Nome,
    int DuracaoMinutos,
    decimal Valor
);