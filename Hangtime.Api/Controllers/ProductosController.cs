using Hangtime.Api.Data;
using Hangtime.Api.Dtos;
using Hangtime.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hangtime.Api.Controllers;

/// <summary>Catálogo público + ABM para administradores.</summary>
[ApiController, Route("api/productos")]
public class ProductosController(StoreDbContext db) : ControllerBase
{
    private static object Mapear(Producto p) => new
    {
        p.Id, p.Nombre, p.Descripcion, p.Precio, p.Categoria,
        p.Color1, p.Color2, p.Color3, p.Destacado,
        Talles = p.Talles.OrderBy(t => t.Numero).Select(t => new { t.Id, t.Numero, t.Stock })
    };

    // GET /api/productos?q=&categoria=&talle=&max=&orden=
    [HttpGet]
    public async Task<IActionResult> Listar([FromQuery] string? q, [FromQuery] string? categoria,
        [FromQuery] int? talle, [FromQuery] decimal? max, [FromQuery] string? orden)
    {
        var query = db.Productos.AsNoTracking().Include(p => p.Talles).Where(p => p.Activo);

        if (!string.IsNullOrWhiteSpace(q)) query = query.Where(p => p.Nombre.Contains(q));
        if (!string.IsNullOrWhiteSpace(categoria)) query = query.Where(p => p.Categoria == categoria);
        if (talle is not null) query = query.Where(p => p.Talles.Any(t => t.Numero == talle && t.Stock > 0));
        if (max is not null) query = query.Where(p => p.Precio <= max);

        query = orden switch
        {
            "precio_asc" => query.OrderBy(p => p.Precio),
            "precio_desc" => query.OrderByDescending(p => p.Precio),
            _ => query.OrderByDescending(p => p.Destacado).ThenByDescending(p => p.Id)
        };

        var lista = await query.ToListAsync();
        return Ok(lista.Select(Mapear));
    }

    // GET /api/productos/3
    [HttpGet("{id:int}")]
    public async Task<IActionResult> Detalle(int id)
    {
        var p = await db.Productos.AsNoTracking().Include(x => x.Talles)
            .FirstOrDefaultAsync(x => x.Id == id && x.Activo);
        return p is null ? NotFound() : Ok(Mapear(p));
    }

    // POST /api/productos  (solo Admin)
    [HttpPost, Authorize(Roles = "Admin")]
    public async Task<IActionResult> Crear(ProductoDto dto)
    {
        if (dto.Talles.GroupBy(t => t.Numero).Any(g => g.Count() > 1))
            return BadRequest(new { error = "Hay talles repetidos." });

        var p = new Producto();
        Aplicar(p, dto);
        db.Productos.Add(p);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(Detalle), new { id = p.Id }, Mapear(p));
    }

    // PUT /api/productos/3  (solo Admin)
    [HttpPut("{id:int}"), Authorize(Roles = "Admin")]
    public async Task<IActionResult> Editar(int id, ProductoDto dto)
    {
        var p = await db.Productos.Include(x => x.Talles).FirstOrDefaultAsync(x => x.Id == id);
        if (p is null) return NotFound();
        Aplicar(p, dto);
        await db.SaveChangesAsync();
        return Ok(Mapear(p));
    }

    // DELETE /api/productos/3  -> baja lógica (solo Admin)
    [HttpDelete("{id:int}"), Authorize(Roles = "Admin")]
    public async Task<IActionResult> Baja(int id)
    {
        var p = await db.Productos.FindAsync(id);
        if (p is null) return NotFound();
        p.Activo = false;
        await db.SaveChangesAsync();
        return NoContent();
    }

    private static void Aplicar(Producto p, ProductoDto dto)
    {
        p.Nombre = dto.Nombre.Trim();
        p.Descripcion = dto.Descripcion?.Trim();
        p.Precio = dto.Precio;
        p.Categoria = dto.Categoria.Trim().ToLowerInvariant();
        p.Color1 = dto.Color1; p.Color2 = dto.Color2; p.Color3 = dto.Color3;
        p.Destacado = dto.Destacado;

        foreach (var t in dto.Talles)
        {
            var existente = p.Talles.FirstOrDefault(x => x.Numero == t.Numero);
            if (existente is null) p.Talles.Add(new Talle { Numero = t.Numero, Stock = t.Stock });
            else existente.Stock = t.Stock;
        }
    }
}
