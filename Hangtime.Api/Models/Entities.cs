namespace Hangtime.Api.Models;

/// <summary>Cliente o administrador. La contraseña se guarda solo como hash BCrypt.</summary>
public class Usuario
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    /// <summary>Cliente o Admin.</summary>
    public string Rol { get; set; } = "Cliente";
    public DateTime CreadoEn { get; set; } = DateTime.UtcNow;
    public int IntentosFallidos { get; set; }
    public DateTime? BloqueadoHasta { get; set; }
    public List<Pedido> Pedidos { get; set; } = new();
}

/// <summary>Modelo de zapatilla. Los colores alimentan la ilustración SVG del front.</summary>
public class Producto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public decimal Precio { get; set; }
    public string Categoria { get; set; } = "retro";
    public string Color1 { get; set; } = "#111111";
    public string Color2 { get; set; } = "#f6f5f2";
    public string Color3 { get; set; } = "#d6001c";
    public bool Destacado { get; set; }
    /// <summary>Baja lógica: los productos con ventas no se borran.</summary>
    public bool Activo { get; set; } = true;
    public DateTime CreadoEn { get; set; } = DateTime.UtcNow;
    public List<Talle> Talles { get; set; } = new();
}

/// <summary>Stock de un modelo en un número (talle) puntual.</summary>
public class Talle
{
    public int Id { get; set; }
    public int ProductoId { get; set; }
    public Producto Producto { get; set; } = null!;
    public int Numero { get; set; }
    public int Stock { get; set; }
}

public class Pedido
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public Usuario Usuario { get; set; } = null!;
    public string Direccion { get; set; } = string.Empty;
    /// <summary>Pendiente, Pagado, Enviado, Entregado o Cancelado.</summary>
    public string Estado { get; set; } = "Pendiente";
    public decimal Total { get; set; }
    public DateTime CreadoEn { get; set; } = DateTime.UtcNow;
    public List<ItemPedido> Items { get; set; } = new();
}

public class ItemPedido
{
    public int Id { get; set; }
    public int PedidoId { get; set; }
    public Pedido Pedido { get; set; } = null!;
    public int TalleId { get; set; }
    public Talle Talle { get; set; } = null!;
    public int Cantidad { get; set; }
    /// <summary>Precio congelado al momento de la compra.</summary>
    public decimal PrecioUnitario { get; set; }
}
