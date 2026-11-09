namespace AgendAi.Application.Profissionais.AlterarMeuProcedimento;

using AgendAi.Application.Profissionais.Ports;
using AgendAi.Application.Profissionais.Models;
using AgendAi.Application.Auth.Ports;
using AgendAi.Application.Auth.Services;
using AgendAi.Application.Auth.Permissions;
using AgendAi.Application.Common.Exceptions;

public sealed class AlterarMeuProcedimentoHandler
{
    private readonly IAlterarVinculoProcedimentoProfissionalWriter _writer;
    private readonly IVinculoProcedimentoProfissionalReader _reader;
    private readonly IContextUsuarioAtual _context;
    private readonly AutorizacaoService _autorizacaoService;

    public AlterarMeuProcedimentoHandler(
        IAlterarVinculoProcedimentoProfissionalWriter writer,
        IVinculoProcedimentoProfissionalReader reader,
        IContextUsuarioAtual context,
        AutorizacaoService autorizacaoService
    )
    {
        _writer = writer;
        _reader = reader;
        _context = context;
        _autorizacaoService = autorizacaoService;
    }

    public async Task<AlterarMeuProcedimentoResult> HandleAsync(
        AlterarMeuProcedimentoCommand command,
        CancellationToken cancellationToken
    )
    {
        ArgumentNullException.ThrowIfNull(command);

        ValidarCommand(command);

        var usuarioAtual = _context.Obter();

        _autorizacaoService.ExigirPermissao(
            usuarioAtual,
            PermissoesUsuario.ProcedimentosPropriosGerenciar
        );

        var profissionalUuid = usuarioAtual.ProfissionalUuid
            ?? throw new AcessoNegadoException();

        if (profissionalUuid == Guid.Empty)
        {
            throw new AcessoNegadoException();
        }

        var procedimento = await _reader.ConsultarPorUuidAsync(
            usuarioAtual.ClinicaUuid,
            profissionalUuid,
            command.ProfissionalProcedimentoUuid,
            cancellationToken
        ) ?? throw new RecursoNaoEncontradoException(
            "procedimento_nao_encontrado",
            "O vinculo profissional-procedimento informado não foi encontrado."
        );

        var duracaoEfetiva = command.DuracaoMinutos 
            ?? procedimento.DuracaoMinutos
            ?? procedimento.DuracaoEstimadaMinutos;

        var valorEfetivo = command.Valor
            ?? procedimento.Valor
            ?? procedimento.ValorBase;

        var disponivelAgendamento = command.DisponivelAgendamento ?? procedimento.VinculoAtivo;
        DateTime? deletedAt = disponivelAgendamento
            ? null
            : (procedimento.DeletedAt ?? DateTime.UtcNow);

        var alterado = await _writer.AlterarAsync(
            new DadosAlteracaoVinculoProcedimentoProfissional(
                ClinicaUuid: usuarioAtual.ClinicaUuid,
                ProfissionalProcedimentoUuid: command.ProfissionalProcedimentoUuid,
                DuracaoMinutos: duracaoEfetiva,
                Valor: valorEfetivo,
                DeletedAt: deletedAt
            ),
            cancellationToken
        );

        if (!alterado)
        {
            throw new RecursoNaoEncontradoException(
                "vinculo_nao_encontrado",
                "O vinculo entre profissional e procedimento não foi encontrado para reativação."
            );
        }

        return new AlterarMeuProcedimentoResult(
            ProfissionalProcedimentoUuid: command.ProfissionalProcedimentoUuid,
            DisponivelAgendamento: disponivelAgendamento,
            Nome: procedimento.Nome,
            DuracaoMinutos: duracaoEfetiva,
            Valor: valorEfetivo
        );
    }

    private static void ValidarCommand(AlterarMeuProcedimentoCommand command)
    {
        if (command.ProfissionalProcedimentoUuid == Guid.Empty)
        {
            throw new ValidacaoException(
                "profissional_procedimento_uuid_invalido",
                "O UUID do vínculo do procedimento é obrigatório."
            );
        }
        if (command.DuracaoMinutos.HasValue && command.DuracaoMinutos.Value <= 0)
        {
            throw new ValidacaoException(
                "duracao_invalida",
                "A duração do procedimento deve ser maior que zero."
            );
        }
        if (command.Valor.HasValue && command.Valor.Value < 0)
        {
            throw new ValidacaoException(
                "valor_invalido",
                "O valor do procedimento não pode ser negativo."
            );
        }
    }
}