namespace AgendAi.Application.Auth.Ports;

public interface IVerificadorSenha
{
    bool Verificar(
        string senhaInput,
        string senhaHash
    );
}