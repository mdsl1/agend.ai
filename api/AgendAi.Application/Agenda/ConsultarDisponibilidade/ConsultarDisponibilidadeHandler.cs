namespace AgendAi.Application.Agenda.ConsultarDisponibilidade;

using AgendAi.Application.Agenda.Models;
using AgendAi.Application.Agenda.Ports;
using AgendAi.Application.Common.Exceptions;
using AgendAi.Application.Agenda.Services;

public sealed class ConsultarDisponibilidadeHandler
{
    private const int LimiteMinimo = 1;
    private const int LimiteMaximo = 3;
    private const string TimezonePadrao = "America/Sao_Paulo";

    private static readonly TimeZoneInfo TimezoneApp = TimeZoneInfo.FindSystemTimeZoneById(TimezonePadrao);

    private readonly IDisponibilidadeReader _disponibilidadeReader;
    private readonly VerificarDisponibilidadeService _verificarDisponibilidadeService;
    
    public ConsultarDisponibilidadeHandler(
        IDisponibilidadeReader disponibilidadeReader,
        VerificarDisponibilidadeService verificarDisponibilidadeService
    )
    {
        _disponibilidadeReader = disponibilidadeReader;
        _verificarDisponibilidadeService = verificarDisponibilidadeService;
    }

    public async Task<ConsultarDisponibilidadeResult> HandleAsync(
        ConsultarDisponibilidadeQuery query,
        CancellationToken cancellationToken
    )
    {
        ArgumentNullException.ThrowIfNull(query);

        ValidarQuery(query);

        var dados = await _disponibilidadeReader.ObterAsync(
            query.ProfissionalUuid,
            query.ProfissionalProcedimentoUuid,
            cancellationToken
        ) ?? throw new RecursoNaoEncontradoException(
            codigo: "profissional_procedimento_nao_encontrado",
            mensagem: "O procedimento informado não está disponivel para este profissional."
        );

        var resultExterno = await _verificarDisponibilidadeService.VerificarAsync(
            dados,
            query.Inicio,
            query.Limite,
            cancellationToken
        );

        var horariosDisponiveis = resultExterno
            .HorariosDisponiveis
            .Select(horario => new HorarioDisponivelResult(
                Inicio: horario.Inicio,
                Fim: horario.Fim
            ))
        .ToArray();

        return new ConsultarDisponibilidadeResult(
            Disponivel: resultExterno.Disponivel,
            DuracaoMinutos: dados.DuracaoMinutos,
            HorariosDisponiveis: horariosDisponiveis,
            BuscaEsgotada: resultExterno.BuscaEsgotada
        );
    }

    private static void ValidarQuery(ConsultarDisponibilidadeQuery query)
    {
        if (query.ProfissionalUuid == Guid.Empty)
        {
            throw new ValidacaoException(
                codigo: "profissional_uuid_invalido",
                mensagem: "O UUID do profissional é obrigatório."
            );
        }

        if (query.ProfissionalProcedimentoUuid == Guid.Empty)
        {
            throw new ValidacaoException(
                codigo: "profissional_procedimento_uuid_invalido",
                mensagem: "O UUID do procedimento é obrigatório."
            );
        }

        if (query.Inicio == default)
        {
            throw new ValidacaoException(
                codigo: "inicio_obrigatorio",
                mensagem: "O inicio solicitado é obrigatório."
            );
        }

        if (query.Limite < LimiteMinimo || query.Limite > LimiteMaximo)
        {
            throw new ValidacaoException(
                codigo: "limite_invalido",
                mensagem: $"O limite de alternativas deve estar entre {LimiteMinimo} e {LimiteMaximo}."
            );
        }
    }
}