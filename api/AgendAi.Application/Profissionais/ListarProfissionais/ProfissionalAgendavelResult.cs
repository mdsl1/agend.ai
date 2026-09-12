namespace AgendAi.Application.Profissionais.ListarProfissionais;

public sealed record ProfissionalAgendavelResult(
    Guid ProfissionalUuid,
    string NomeExibicao,
    EspecialidadeProfissionalResult? Especialidade
);