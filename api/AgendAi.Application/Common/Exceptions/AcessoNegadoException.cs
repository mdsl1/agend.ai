namespace AgendAi.Application.Common.Exceptions;

public sealed class AcessoNegadoException : AplicacaoException
{
    public AcessoNegadoException()
    :base(
        "acesso_negado",
        "Acesso negado"
    ){}
}