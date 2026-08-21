namespace AgendAi.Application.Common.Exceptions;

public sealed class RecursoNaoEncontradoException : AplicacaoException
{
    public RecursoNaoEncontradoException(string codigo, string mensagem) : base(codigo, mensagem)
    {
        
    }
}