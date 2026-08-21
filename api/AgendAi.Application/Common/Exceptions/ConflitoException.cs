namespace AgendAi.Application.Common.Exceptions;

public sealed class ConflitoException : AplicacaoException
{
    public ConflitoException(string codigo, string mensagem) : base(codigo, mensagem)
    {
        
    }
}