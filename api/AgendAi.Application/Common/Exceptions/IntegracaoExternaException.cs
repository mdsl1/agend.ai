namespace AgendAi.Application.Common.Exceptions;

public sealed class IntegracaoExternaException : AplicacaoException
{
    public IntegracaoExternaException(
        string codigo,
        string mensagem,
        Exception? innerException = null
    ) : base(codigo, mensagem, innerException)
    {
        
    }
}