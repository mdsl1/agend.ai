namespace AgendAi.Application.Auth.Ports;

using AgendAi.Application.Auth.Models;

public interface IGeradorAccessToken
{
    AccessTokenGerado Gerar(
        DadosGeracaoAccessToken dados
    );
}