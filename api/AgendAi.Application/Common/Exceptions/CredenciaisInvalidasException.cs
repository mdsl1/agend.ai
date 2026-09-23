namespace AgendAi.Application.Common.Exceptions;

public sealed class CredenciaisInvalidasException : AplicacaoException
{
    public CredenciaisInvalidasException()
    : base(
        "credenciais_invalidas",
        "Credenciais invalidas."
    ){}
}