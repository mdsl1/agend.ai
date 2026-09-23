namespace AgendAi.Application.Auth.Login;

using AgendAi.Application.Auth.Models;
using AgendAi.Application.Auth.Ports;
using AgendAi.Application.Common.Exceptions;

public sealed class LoginHandler
{
    private const int TamanhoMaximoEmail = 150;

    private readonly IAutenticacaoUsuarioReader _autenticacaoUsuarioReader;
    private readonly IVerificadorSenha _verificadorSenha;
    private readonly IGeradorAccessToken _geradorAccessToken;

    public LoginHandler(
        IAutenticacaoUsuarioReader autenticacaoUsuarioReader,
        IVerificadorSenha verificadorSenha,
        IGeradorAccessToken geradorAccessToken
    )
    {
        _autenticacaoUsuarioReader = autenticacaoUsuarioReader;
        _verificadorSenha = verificadorSenha;
        _geradorAccessToken = geradorAccessToken;
    }

    public async Task<LoginResult> HandleAsync(
        LoginCommand command,
        CancellationToken cancellationToken
    )
    {
        ArgumentNullException.ThrowIfNull(command);
        
        ValidarCommand(command);

        var email = command.Email.Trim().ToLowerInvariant();

        var usuario = await _autenticacaoUsuarioReader.BuscarPorClinicaEEmailAsync(
            command.ClinicaUuid,
            email,
            cancellationToken
        );

        if (usuario is null)
        {
            throw new CredenciaisInvalidasException();
        }

        var senhaValida = _verificadorSenha.Verificar(
            command.Senha,
            usuario.SenhaHash
        );

        if (!senhaValida)
        {
            throw new CredenciaisInvalidasException();
        }

        var token = _geradorAccessToken.Gerar(
            new DadosGeracaoAccessToken(
                usuario.UsuarioUuid,
                usuario.ClinicaUuid,
                usuario.Cargo,
                usuario.IsAdmin,
                usuario.ProfissionalUuid
            )
        );

        var usuarioResult = new UsuarioAutenticadoResult(
            usuario.UsuarioUuid,
            usuario.Nome,
            usuario.Cargo,
            usuario.IsAdmin,
            usuario.ProfissionalUuid,
            usuario.ClinicaUuid
        );

        return new LoginResult(
            token.JWT,
            token.ExpiraEm,
            usuarioResult
        );
    }

    private static void ValidarCommand(LoginCommand command)
    {
        if (command.ClinicaUuid == Guid.Empty)
        {
            throw new ValidacaoException(
                codigo: "clinica_uuid_invalida",
                mensagem: "O UUID da clinica é obrigatório."
            );
        }

        if (string.IsNullOrWhiteSpace(command.Email))
        {
            throw new ValidacaoException(
                codigo: "email_obrigatorio",
                mensagem: "O e-mail é obrigatório."
            );
        }

        if (command.Email.Trim().Length > TamanhoMaximoEmail)
        {
            throw new ValidacaoException(
                codigo: "email_invalido",
                mensagem: $"O e-mail deve ter no máximo {TamanhoMaximoEmail} caracteres."
            );
        }

        if (string.IsNullOrWhiteSpace(command.Senha))
        {
            throw new ValidacaoException(
                codigo: "senha_obrigatoria",
                mensagem: "A senha é obrigatória."
            );
        }
    }
}