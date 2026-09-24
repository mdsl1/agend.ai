namespace AgendAi.Application.Auth.Ports;

using AgendAi.Application.Auth.Models;

public interface IContextUsuarioAtual
{
    DadosUsuarioAtual Obter();
}