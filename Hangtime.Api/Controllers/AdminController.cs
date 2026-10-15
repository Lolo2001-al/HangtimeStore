using Hangtime.Api.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hangtime.Api.Controllers;

/// <summary>Indicadores para el panel de administración.</summary>
[ApiController, Route("api/admin"), Authorize(Roles = "Admin")]
public class AdminController(StoreDbContext db) : ControllerBase
{
    // GET /api/admin/resumen
    [HttpGet("resumen")]
    public async Task<IActionResult> Resumen()
    {
        var validos = db.Pedidos.Where(p => p.Estado != "Cancelado");

        var ventas = await validos.SumAsync(p => (decimal?)p.Total) ?? 0m;
        var pedidos = await validos.CountAsync();

        var top = await db.Items.Where(i => i.Pedido.Estado != "Cancelado")
            .GroupBy(i => i.Talle.Producto.Nombre)
            .Select(g => new { Producto = g.Key, Unidades = g.Sum(i => i.Cantidad) })
            .OrderByDescending(x => x.Unidades).Take(5).ToListAsync();

        var stockBajo = await db.Talles.Where(t => t.Stock <= 1 && t.Producto.Activo)
            .OrderBy(t => t.Stock)
            .Select(t => new { Producto = t.Producto.Nombre, Talle = t.Numero, t.Stock })
            .Take(10).ToListAsync();

        return Ok(new { ventas, pedidos, top, stockBajo });
    }
}
