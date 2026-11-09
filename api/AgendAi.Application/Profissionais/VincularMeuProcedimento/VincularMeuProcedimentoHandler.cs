namespace AgendAi.Application.Profissionais.VincularMeuProcedimento;

using AgendAi.Application.Profissionais.Ports;
using AgendAi.Application.Profissionais.Models;
using AgendAi.Application.Auth.Ports;
using AgendAi.Application.Auth.Services;
using AgendAi.Application.Auth.Permissions;
using AgendAi.Application.Common.Exceptions;

public sealed class VincularMeuProcedimentoHandler
{
    private readonly IAlterarVinculoProcedimentoProfissionalWriter _alterarWriter;
    private readonly ICriarVinculoProcedimentoProfissionalWriter _criarWriter;
    private readonly IVinculoProcedimentoProfissionalReader _reader;
    private readonly IContextUsuarioAtual _context;
    private readonly AutorizacaoService _autorizacaoService;

    public VincularMeuProcedimentoHandler(
        IAlterarVinculoProcedimentoProfissionalWriter alterarWriter,
        ICriarVinculoProcedimentoProfissionalWriter criarWriter,
        IVinculoProcedimentoProfissionalReader reader,
        IContextUsuarioAtual context,
        AutorizacaoService autorizacaoService
    )
    {
        _alterarWriter = alterarWriter;
        _criarWriter = criarWriter;
        _reader = reader;
        _context = context;
        _autorizacaoService = autorizacaoService;
    }

    public async Task<VincularMeuProcedimentoResult> HandleAsync(
        VincularMeuProcedimentoCommand command,
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

        var procedimento = await _reader.ConsultarAsync(
            usuarioAtual.ClinicaUuid,
            profissionalUuid,
            command.ProcedimentoUuid,
            cancellationToken
        ) ?? throw new RecursoNaoEncontradoException(
            "procedimento_nao_encontrado",
            "O procedimento informado não foi encontrado."
        );

        if (procedimento.VinculoAtivo)
        {
            throw new ConflitoException(
                "vinculo_existente",
                "Esse profissional já possui esse procedimento ativo no seu catálogo pessoal."
            );
        }

        var duracaoEfetiva = command.DuracaoMinutos ?? procedimento.DuracaoEstimadaMinutos;
        var valorEfetivo = command.Valor ?? procedimento.ValorBase;

        if (procedimento.VinculoExistente)
        {
            var profissionalProcedimentoUuid = procedimento.ProfissionalProcedimentoUuid!.Value;

            var alterado = await _alterarWriter.AlterarAsync(
                new DadosAlteracaoVinculoProcedimentoProfissional(
                    ClinicaUuid: usuarioAtual.ClinicaUuid,
                    ProfissionalProcedimentoUuid: profissionalProcedimentoUuid,
                    DuracaoMinutos: duracaoEfetiva,
                    Valor: valorEfetivo,
                    DeletedAt: null
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

            return new VincularMeuProcedimentoResult(
                ProfissionalProcedimentoUuid: profissionalProcedimentoUuid,
                Criado: false,
                Nome: procedimento.Nome,
                DuracaoMinutos: duracaoEfetiva,
                Valor: valorEfetivo
            );
        }

        var novoVinculoUuid = await _criarWriter.InserirAsync(
            new DadosCriacaoVinculoProcedimentoProfissional(
                ClinicaUuid: usuarioAtual.ClinicaUuid,
                ProfissionalUuid: profissionalUuid,
                ProcedimentoUuid: command.ProcedimentoUuid,
                DuracaoMinutos: duracaoEfetiva,
                Valor: valorEfetivo
            ),
            cancellationToken
        );

        return new VincularMeuProcedimentoResult(
            ProfissionalProcedimentoUuid: novoVinculoUuid,
            Criado: true,
            Nome: procedimento.Nome,
            DuracaoMinutos: duracaoEfetiva,
            Valor: valorEfetivo
        );
    }

    private static void ValidarCommand(VincularMeuProcedimentoCommand command)
    {
        if (command.ProcedimentoUuid == Guid.Empty)
        {
            throw new ValidacaoException(
                "procedimento_uuid_invalido",
                "O UUID do procedimento é obrigatório."
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
