using Hangtime.Api.Data;
using Hangtime.Api.Dtos;
using Hangtime.Api.Models;
using Hangtime.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace Hangtime.Api.Controllers;

/// <summary>Registro e inicio de sesión. Devuelve un JWT.</summary>
[ApiController, Route("api/auth"), EnableRateLimiting("auth")]
public class AuthController(StoreDbContext db, TokenService tokens) : ControllerBase
{
    private const int MaxIntentos = 5;

    private static bool PasswordFuerte(string p) =>
        p.Any(char.IsUpper) && p.Any(char.IsLower) && p.Any(char.IsDigit);

    private object Respuesta(Usuario u) => new { token = tokens.Crear(u), nombre = u.Nombre, rol = u.Rol };

    // POST /api/auth/registro
    [HttpPost("registro")]
    public async Task<IActionResult> Registro(RegistroDto dto)
    {
        if (!PasswordFuerte(dto.Password))
            return BadRequest(new { error = "La contraseña necesita mayúscula, minúscula y número." });

        var email = dto.Email.Trim().ToLowerInvariant();
        if (await db.Usuarios.AnyAsync(u => u.Email == email))
            return Conflict(new { error = "Ese email ya está registrado." });

        var usuario = new Usuario
        {
            Nombre = dto.Nombre.Trim(),
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password, 12)
        };
        db.Usuarios.Add(usuario);
        await db.SaveChangesAsync();
        return Ok(Respuesta(usuario));
    }

    // POST /api/auth/login  -> bloqueo temporal tras 5 intentos fallidos
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var email = dto.Email.Trim().ToLowerInvariant();
        var u = await db.Usuarios.FirstOrDefaultAsync(x => x.Email == email);

        if (u is not null && u.BloqueadoHasta > DateTime.UtcNow)
            return StatusCode(423, new { error = "Cuenta bloqueada temporalmente. Probá en unos minutos." });

        // Mismo mensaje exista o no el usuario: no se revela qué emails están registrados.
        if (u is null || !BCrypt.Net.BCrypt.Verify(dto.Password, u.PasswordHash))
        {
            if (u is not null)
            {
                u.IntentosFallidos++;
                if (u.IntentosFallidos >= MaxIntentos)
                {
                    u.BloqueadoHasta = DateTime.UtcNow.AddMinutes(10);
                    u.IntentosFallidos = 0;
                }
                await db.SaveChangesAsync();
            }
            return Unauthorized(new { error = "Email o contraseña incorrectos." });
        }

        u.IntentosFallidos = 0;
        u.BloqueadoHasta = null;
        await db.SaveChangesAsync();
        return Ok(Respuesta(u));
    }
}
