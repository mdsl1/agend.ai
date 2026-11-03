namespace AgendAi.Domain.Common.Exceptions;

public sealed class RegraNegocioException : Exception
{
    public RegraNegocioException(string codigo, string mensagem) : base(mensagem)
    {
        Codigo = codigo;
    }
    public string Codigo { get; }
}