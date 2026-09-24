namespace AgendAi.Application.Common.Exceptions;

public sealed class NaoAutenticadoException : AplicacaoException
{
    public NaoAutenticadoException() : base (
        "context_autenticacao_invalido",
        "Não foi possível identificar o usuário autenticado."
    ){}
}