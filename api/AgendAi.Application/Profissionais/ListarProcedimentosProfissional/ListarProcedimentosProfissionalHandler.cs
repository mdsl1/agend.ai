namespace AgendAi.Application.Profissionais.ListarProcedimentosProfissional;

using AgendAi.Application.Common.Exceptions;
using AgendAi.Application.Profissionais.ListarProcedimentosProfissional.Ports;

public sealed class ListarProcedimentosProfissionalHandler
{
    private readonly IProcedimentosProfissionalReader _procedimentosProfissionalReader;

    public ListarProcedimentosProfissionalHandler(IProcedimentosProfissionalReader procedimentosProfissionalReader)
    {
        _procedimentosProfissionalReader = procedimentosProfissionalReader;
    }

    public async Task<ListarProcedimentosProfissionalResult> HandleAsync(
        ListarProcedimentosProfissionalQuery query,
        CancellationToken cancellationToken
    )
    {
        ArgumentNullException.ThrowIfNull(query);

        ValidarQuery(query);

        var dados = await _procedimentosProfissionalReader.ListarAsync(
            query.ProfissionalUuid,
            cancellationToken
        ) ?? throw new RecursoNaoEncontradoException(
            codigo: "profissional_nao_encontrado",
            mensagem: "O profissional informado não foi encontrado."
        );

        var procedimentos = dados.Procedimentos
            .Select(procedimento => new ProcedimentoProfissionalResult(
                ProfissionalProcedimentoUuid: procedimento.ProfissionalProcedimentoUuid,
                ProcedimentoUuid: procedimento.ProcedimentoUuid,
                Nome: procedimento.Nome,
                ValorEfetivo: procedimento.Valor,
                DuracaoEfetivaMinutos: procedimento.DuracaoMinutos
            )).ToArray();

        return new ListarProcedimentosProfissionalResult(Procedimentos: procedimentos);
    }

    private static void ValidarQuery(ListarProcedimentosProfissionalQuery query)
    {
        if (query.ProfissionalUuid == Guid.Empty)
        {
            throw new ValidacaoException(
                codigo: "profissional_uuid_invalido",
                mensagem: "O UUID do profissional é obrigatório."
            );
        }
    }
}