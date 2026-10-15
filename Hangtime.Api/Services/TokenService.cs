using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Hangtime.Api.Models;
using Microsoft.IdentityModel.Tokens;

namespace Hangtime.Api.Services;

/// <summary>Genera el JWT firmado (HS256) con id, email y rol del usuario.</summary>
public class TokenService(IConfiguration config)
{
    public string Crear(Usuario u)
    {
        var jwt = config.GetSection("Jwt");
        var clave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!));
        var claims = new[]
        {
            new Claim("sub", u.Id.ToString()),
            new Claim("email", u.Email),
            new Claim("role", u.Rol)
        };
        var token = new JwtSecurityToken(
            issuer: jwt["Issuer"], audience: jwt["Audience"], claims: claims,
            expires: DateTime.UtcNow.AddMinutes(int.Parse(jwt["Minutos"] ?? "60")),
            signingCredentials: new SigningCredentials(clave, SecurityAlgorithms.HmacSha256));
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
