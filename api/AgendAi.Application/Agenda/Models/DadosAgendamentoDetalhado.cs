namespace AgendAi.Application.Agenda.Models;

public sealed record DadosAgendamentoDetalhado(
    Guid AgendamentoUuid,
    Guid ClinicaUuid,
    Guid ClienteUuid,
    string NomeCliente,
    string Telefone,
    Guid ProfissionalUuid,
    string NomeExibicaoProfissional,
    Guid ProcedimentoUuid,
    string NomeProcedimento,
    int DuracaoMinutos,
    DateTimeOffset Inicio,
    DateTimeOffset Fim,
    string? MotivoContato,
    string? AnotacoesProfissional,
    decimal ValorTotal,
    string Status,
    string StatusPagamento
);