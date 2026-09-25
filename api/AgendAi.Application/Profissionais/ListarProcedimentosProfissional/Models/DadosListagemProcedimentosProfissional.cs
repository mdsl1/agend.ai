namespace AgendAi.Application.Profissionais.ListarProcedimentosProfissional.Models;

public sealed record DadosListagemProcedimentosProfissional(
    Guid ProfissionalUuid,
    Guid ClinicaUuid,
    IReadOnlyCollection<DadosProcedimentoProfissional> Procedimentos
);