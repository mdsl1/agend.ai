namespace AgendAi.Infrastructure.Auth;

using AgendAi.Application.Auth.Ports;
using Microsoft.AspNetCore.Identity;

public sealed class VerificadorSenha : IVerificadorSenha
{
    private readonly PasswordHasher<object> _passwordHasher = new();
    private readonly object _contextUsuario = new();

    public bool Verificar(
        string senhaInput,
        string senhaHash
    )
    {
        if (string.IsNullOrWhiteSpace(senhaInput) || string.IsNullOrWhiteSpace(senhaHash))
        {
            return false;
        }

        try
        {
            var result = _passwordHasher.VerifyHashedPassword(
                _contextUsuario,
                senhaHash,
                senhaInput
            );

            return result != PasswordVerificationResult.Failed;
        }
        catch(FormatException)
        {
            return false;
        }
    }
}