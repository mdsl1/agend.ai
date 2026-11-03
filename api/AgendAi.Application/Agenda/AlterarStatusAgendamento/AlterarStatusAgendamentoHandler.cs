namespace AgendAi.Application.Agenda.AlterarStatusAgendamento;

using AgendAi.Application.Agenda.Ports;
using AgendAi.Application.Common.Exceptions;
using AgendAi.Application.Auth.Permissions;
using AgendAi.Application.Auth.Ports;
using AgendAi.Application.Auth.Services;
using AgendAi.Domain.Agendamentos;
using AgendAi.Domain.Common.Exceptions;
using AgendAi.Application.Agenda.Models;

public sealed class AlterarStatusAgendamentoHandler
{
    private readonly IAgendamentoStatusWriter _writer;
    private readonly IAgendamentoDetalhadoReader _reader;
    private readonly IContextUsuarioAtual _context;
    private readonly AutorizacaoService _autorizacaoService;
    
    public AlterarStatusAgendamentoHandler(
        IAgendamentoStatusWriter writer,
        IAgendamentoDetalhadoReader reader,
        IContextUsuarioAtual context,
        AutorizacaoService autorizacaoService
    )
    {
        _writer = writer;
        _reader = reader;
        _context = context;
        _autorizacaoService = autorizacaoService;
    }

    public async Task<AlterarStatusAgendamentoResult> HandleAsync(
        AlterarStatusAgendamentoCommand command,
        CancellationToken cancellationToken
    )
    {
        ArgumentNullException.ThrowIfNull(command);

        ValidarCommand(command);

        var usuarioAtual = _context.Obter();

        var agendamento = await _reader.ConsultarAsync(
            command.AgendamentoUuid,
            usuarioAtual.ClinicaUuid,
            cancellationToken
        ) ?? throw new RecursoNaoEncontradoException(
            "agendamento_nao_encontrado",
            "O agendamento informado não foi encontrado para alteração."
        );

        if (usuarioAtual.IsAdmin)
        {
            _autorizacaoService.ExigirMesmaClinica(
                usuarioAtual,
                agendamento.ClinicaUuid
            );
        }
        else
        {
            _autorizacaoService.ExigirAcessoAoProfissional(
                usuarioAtual,
                agendamento.ClinicaUuid,
                agendamento.ProfissionalUuid,
                PermissoesUsuario.AgendamentosPropriosGerenciar,
                PermissoesUsuario.AgendamentosClinicaGerenciar
            );
        }

        ResultadoAlteracaoStatusAgendamento? resultado;
        try
        {  
            resultado = await _writer.AlterarAsync(
                command.AgendamentoUuid,
                usuarioAtual.ClinicaUuid,
                command.NovoStatus,
                cancellationToken
            ) ?? throw new RecursoNaoEncontradoException(
                "agendamento_nao_encontrado",
                "O agendamento informado não foi encontrado para alteração."
            );
        }
        catch (RegraNegocioException exception)
        {
            throw new ConflitoException(
                exception.Codigo,
                exception.Message,
                exception
            );
        }

        return new AlterarStatusAgendamentoResult(
            AgendamentoUuid: resultado.AgendamentoUuid,
            StatusAnterior: resultado.StatusAnterior,
            NovoStatus: resultado.NovoStatus,
            AlteradoEm: resultado.AlteradoEm
        );
    }

    private static void ValidarCommand(AlterarStatusAgendamentoCommand command)
    {
        if (command.AgendamentoUuid == Guid.Empty)
        {
            throw new ValidacaoException(
                "agendamento_uuid_invalido",
                "O UUID do agendamento é obrigatório."
            );
        }

        var statusPermitidos = new List<string>
        {
            Agendamento.StatusConcluido,
            Agendamento.StatusCancelado,
            Agendamento.StatusFaltou
        };

        if (
            string.IsNullOrWhiteSpace(command.NovoStatus)
            || !statusPermitidos.Contains(
                command.NovoStatus, StringComparer.OrdinalIgnoreCase
            )
        )
        {
            throw new ValidacaoException(
                "status_invalido",
                "O novo status do agendamento é obrigatório."
            );
        }
    }
}