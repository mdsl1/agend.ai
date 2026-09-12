namespace AgendAi.Application.Profissionais.Models;

public sealed record DadosProfissionalAgendavel(
    Guid ProfissionalUuid,
    string Nome,
    string? Prefixo,
    Guid? EspecialidadeUuid,
    string? EspecialidadeNome
);