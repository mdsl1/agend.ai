namespace AgendAi.Application.Profissionais.Models;

public sealed record DadosListagemProcedimentosProfissional(
    Guid ProfissionalUuid,
    Guid ClinicaUuid,
    IReadOnlyCollection<DadosProcedimentoProfissional> Procedimentos
);