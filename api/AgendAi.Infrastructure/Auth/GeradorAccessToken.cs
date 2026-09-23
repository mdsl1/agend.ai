namespace AgendAi.Infrastructure.Auth;

using System.Text;
using AgendAi.Application.Auth.Models;
using AgendAi.Application.Auth.Ports;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

public sealed class GeradorAccessToken : IGeradorAccessToken
{
    private readonly JwtOptions _options;
    private readonly JsonWebTokenHandler _tokenHandler = new();

    public GeradorAccessToken(IOptions<JwtOptions> options)
    {
        _options = options.Value;
    }

    public AccessTokenGerado Gerar( DadosGeracaoAccessToken dados)
    {
        var emitidoEm = DateTimeOffset.UtcNow;
        var expiraEm = emitidoEm.AddMinutes(_options.ExpirationMinutes);

        var claims = new Dictionary<string, object>
        {
            [JwtRegisteredClaimNames.Sub] = dados.UsuarioUuid.ToString(),

            [JwtRegisteredClaimNames.Jti] = Guid.NewGuid().ToString(),
            [JwtClaims.ClinicaUuid] = dados.ClinicaUuid.ToString(),

            [JwtClaims.Cargo] = dados.Cargo.ToString(),

            [JwtClaims.IsAdmin] = dados.IsAdmin
        };

        if (dados.ProfissionalUuid.HasValue)
        {
            claims[JwtClaims.ProfissionalUuid] = dados.ProfissionalUuid.Value.ToString();
        }

        var chave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Secret));

        var credenciais = new SigningCredentials(chave, SecurityAlgorithms.HmacSha256);

        var descricaoToken = new SecurityTokenDescriptor
        {
            Issuer = _options.Issuer,
            Audience = _options.Audience,
            Claims = claims,
            IssuedAt = emitidoEm.UtcDateTime,
            NotBefore = emitidoEm.UtcDateTime,
            Expires = expiraEm.UtcDateTime,
            SigningCredentials = credenciais
        };

        var token = _tokenHandler.CreateToken(descricaoToken);

        return new AccessTokenGerado(
            token,
            expiraEm
        );
    }
}