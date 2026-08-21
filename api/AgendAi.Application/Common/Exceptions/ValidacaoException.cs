namespace AgendAi.Application.Common.Exceptions;

public sealed class ValidacaoException : AplicacaoException
{
    public ValidacaoException(string codigo, string mensagem): base(codigo, mensagem)
    {
        
    }
}