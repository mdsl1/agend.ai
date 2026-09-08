namespace AgendAi.Infrastructure.Integracoes.N8n;

using System.Text.Json.Serialization;

internal sealed record ConsultarDisponibilidadeN8nRequest(
    [property: JsonPropertyName("operacao")]
    string Operacao,

    [property: JsonPropertyName("timezone")]
    string Timezone,

    [property: JsonPropertyName("profissional")]
    ProfissionalN8nRequest Profissional,

    [property: JsonPropertyName("periodo_solicitado")]
    PeriodoN8nRequest PeriodoSolicitado,

    [property: JsonPropertyName("janelas_atendimento")]
    IReadOnlyCollection<JanelaAtendimentoN8nRequest> JanelasAtendimento,

    [property: JsonPropertyName("opcoes")]
    OpcoesDisponibilidadeN8nRequest Opcoes
);

internal sealed record JanelaAtendimentoN8nRequest (
    [property: JsonPropertyName("dia_semana")]
    short DiaSemana,

    [property: JsonPropertyName("inicio")]
    DateTimeOffset Inicio,

    [property: JsonPropertyName("fim")]
    DateTimeOffset Fim
);

internal sealed record OpcoesDisponibilidadeN8nRequest (
    [property: JsonPropertyName("quantidade_alternativas")]
    int QuantidadeAlternativas,

    [property: JsonPropertyName("dias_busca")]
    int DiasBusca,

    [property: JsonPropertyName("duracao_sessao_minutos")]
    int DuracaoSessaoMinutos
);

internal sealed record ConsultarDisponibilidadeN8nResponse (
    [property: JsonPropertyName("sucesso")]
    bool Sucesso,

    [property: JsonPropertyName("disponivel")]
    bool Disponivel,

    [property: JsonPropertyName("codigo")]
    string? Codigo,

    [property: JsonPropertyName("profissional_uuid")]
    Guid? ProfissionalUuid,

    [property: JsonPropertyName("periodo_solicitado")]
    PeriodoDisponibilidadeN8nResponse? PeriodoSolicitado,

    [property: JsonPropertyName("alternativas_disponiveis")]
    IReadOnlyCollection<HorarioDisponivelN8nResponse>? AlternativasDisponiveis,

    [property: JsonPropertyName("busca_esgotada")]
    bool BuscaEsgotada
);

internal sealed record PeriodoDisponibilidadeN8nResponse (
    [property: JsonPropertyName("inicio")]
    DateTimeOffset Inicio,

    [property: JsonPropertyName("fim")]
    DateTimeOffset Fim
);

internal sealed record HorarioDisponivelN8nResponse (
    [property: JsonPropertyName("inicio")]
    DateTimeOffset Inicio,

    [property: JsonPropertyName("fim")]
    DateTimeOffset Fim
);


