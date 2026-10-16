namespace AgendAi.Application.Clinicas.ResolverClinica;

using AgendAi.Application.Clinicas.Ports;
using AgendAi.Application.Common.Exceptions;

public sealed class ResolverClinicaHandler
{
    private readonly IResolverClinicaReader _reader;

    public ResolverClinicaHandler(
        IResolverClinicaReader reader
    )
    {
        _reader = reader;
    }

    public async Task<ResolverClinicaResult> ResolverClinicaAsync(
        ResolverClinicaQuery query,
        CancellationToken cancellationToken
    )
    {
        ArgumentNullException.ThrowIfNull(query);

        if (string.IsNullOrWhiteSpace(query.CodigoIdentificador))
        {
            throw new ValidacaoException(
                "codigo_identificador_obrigatorio",
                "O código identificador da clínica é obrigatório"
            );
        }

        var dados = await _reader.ObterAsync(
            query.CodigoIdentificador.Trim(),
            cancellationToken
        ) ?? throw new RecursoNaoEncontradoException(
                "codigo_identificador_nao_encontrado",
                "O código identificador informado não foi encontrado vinculado à nenhuma clínica."
            );

        return new ResolverClinicaResult(
            Uuid: dados.Uuid,
            Nome: dados.Nome
        );
    }
}
