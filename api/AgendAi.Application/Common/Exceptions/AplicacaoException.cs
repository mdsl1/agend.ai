namespace AgendAi.Application.Common.Exceptions;

public abstract class AplicacaoException : Exception
{
    protected AplicacaoException (
        string codigo,
        string mensagem,
        Exception? innerException = null
    ) : base(mensagem, innerException) {
        Codigo = codigo;
    }

    public string Codigo { get; }
}