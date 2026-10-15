using System.Security.Claims;
using Hangtime.Api.Data;
using Hangtime.Api.Dtos;
using Hangtime.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hangtime.Api.Controllers;

/// <summary>Compra, historial y gestión de pedidos.</summary>
[ApiController, Route("api/pedidos"), Authorize]
public class PedidosController(StoreDbContext db) : ControllerBase
{
    private static readonly string[] Estados = ["Pendiente", "Pagado", "Enviado", "Entregado", "Cancelado"];

    private int UsuarioId => int.Parse(User.FindFirstValue("sub")!);

    private IQueryable<Pedido> ConDetalle() => db.Pedidos.AsNoTracking()
        .Include(p => p.Usuario)
        .Include(p => p.Items).ThenInclude(i => i.Talle).ThenInclude(t => t.Producto);

    private static object Mapear(Pedido p) => new
    {
        p.Id, p.Estado, p.Total, p.Direccion, p.CreadoEn,
        Cliente = p.Usuario.Nombre,
        Items = p.Items.Select(i => new { Producto = i.Talle.Producto.Nombre, Talle = i.Talle.Numero, i.Cantidad, i.PrecioUnitario })
    };

    // POST /api/pedidos
    // Transacción: bloquea cada talle (FOR UPDATE), valida stock y descuenta. Si falta stock, nada se guarda.
    [HttpPost]
    public async Task<IActionResult> Crear(CrearPedidoDto dto)
    {
        var lineas = dto.Items.GroupBy(i => i.TalleId)
            .Select(g => (TalleId: g.Key, Cantidad: g.Sum(x => x.Cantidad))).ToList();

        await using var tx = await db.Database.BeginTransactionAsync();
        var pedido = new Pedido { UsuarioId = UsuarioId, Direccion = dto.Direccion.Trim() };

        foreach (var (talleId, cantidad) in lineas)
        {
            // Consulta parametrizada (anti SQL injection) que toma el lock de la fila.
            await db.Database.ExecuteSqlInterpolatedAsync($"SELECT Id FROM Talles WHERE Id = {talleId} FOR UPDATE");

            var talle = await db.Talles.Include(t => t.Producto).FirstOrDefaultAsync(t => t.Id == talleId);
            if (talle is null || !talle.Producto.Activo)
                return BadRequest(new { error = "Un producto del carrito ya no está disponible." });

            if (talle.Stock < cantidad)
                return Conflict(new { error = $"Sin stock suficiente: {talle.Producto.Nombre}, talle {talle.Numero} (quedan {talle.Stock})." });

            talle.Stock -= cantidad;
            // El precio sale de la base, nunca del cliente.
            pedido.Items.Add(new ItemPedido { TalleId = talleId, Cantidad = cantidad, PrecioUnitario = talle.Producto.Precio });
        }

        pedido.Total = pedido.Items.Sum(i => i.PrecioUnitario * i.Cantidad);
        db.Pedidos.Add(pedido);
        await db.SaveChangesAsync();
        await tx.CommitAsync();
        return Ok(new { pedido.Id, pedido.Total });
    }

    // GET /api/pedidos/mios
    [HttpGet("mios")]
    public async Task<IActionResult> Mios()
    {
        var uid = UsuarioId;
        var lista = await ConDetalle().Where(p => p.UsuarioId == uid).OrderByDescending(p => p.CreadoEn).ToListAsync();
        return Ok(lista.Select(Mapear));
    }

    // GET /api/pedidos  (solo Admin)
    [HttpGet, Authorize(Roles = "Admin")]
    public async Task<IActionResult> Todos()
    {
        var lista = await ConDetalle().OrderByDescending(p => p.CreadoEn).Take(100).ToListAsync();
        return Ok(lista.Select(Mapear));
    }

    // PATCH /api/pedidos/5/estado  (solo Admin). Cancelar devuelve el stock.
    [HttpPatch("{id:int}/estado"), Authorize(Roles = "Admin")]
    public async Task<IActionResult> CambiarEstado(int id, EstadoDto dto)
    {
        if (!Estados.Contains(dto.Estado)) return BadRequest(new { error = "Estado inválido." });

        await using var tx = await db.Database.BeginTransactionAsync();
        var pedido = await db.Pedidos.Include(p => p.Items).ThenInclude(i => i.Talle).FirstOrDefaultAsync(p => p.Id == id);
        if (pedido is null) return NotFound();
        if (pedido.Estado == "Cancelado") return BadRequest(new { error = "Un pedido cancelado no se puede reactivar." });

        if (dto.Estado == "Cancelado")
            foreach (var item in pedido.Items) item.Talle.Stock += item.Cantidad;

        pedido.Estado = dto.Estado;
        await db.SaveChangesAsync();
        await tx.CommitAsync();
        return Ok(new { pedido.Id, pedido.Estado });
    }
}
