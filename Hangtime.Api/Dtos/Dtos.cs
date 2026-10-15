using System.ComponentModel.DataAnnotations;

namespace Hangtime.Api.Dtos;

public class RegistroDto
{
    [Required, StringLength(100, MinimumLength = 2)] public string Nombre { get; set; } = "";
    [Required, EmailAddress, StringLength(160)] public string Email { get; set; } = "";
    [Required, StringLength(100, MinimumLength = 8)] public string Password { get; set; } = "";
}

public class LoginDto
{
    [Required, EmailAddress] public string Email { get; set; } = "";
    [Required] public string Password { get; set; } = "";
}

public class TalleDto
{
    [Range(34, 50)] public int Numero { get; set; }
    [Range(0, 1000)] public int Stock { get; set; }
}

public class ProductoDto
{
    [Required, StringLength(120, MinimumLength = 2)] public string Nombre { get; set; } = "";
    [StringLength(500)] public string? Descripcion { get; set; }
    [Range(1, 10000000)] public decimal Precio { get; set; }
    [Required, StringLength(30)] public string Categoria { get; set; } = "retro";
    // Solo hexadecimales: evita inyectar texto arbitrario en el SVG del front (XSS).
    [RegularExpression("^#[0-9a-fA-F]{6}$")] public string Color1 { get; set; } = "#111111";
    [RegularExpression("^#[0-9a-fA-F]{6}$")] public string Color2 { get; set; } = "#f6f5f2";
    [RegularExpression("^#[0-9a-fA-F]{6}$")] public string Color3 { get; set; } = "#d6001c";
    public bool Destacado { get; set; }
    [Required, MinLength(1), MaxLength(15)] public List<TalleDto> Talles { get; set; } = new();
}

public class ItemPedidoDto
{
    public int TalleId { get; set; }
    [Range(1, 5)] public int Cantidad { get; set; }
}

public class CrearPedidoDto
{
    [Required, StringLength(250, MinimumLength = 5)] public string Direccion { get; set; } = "";
    [Required, MinLength(1), MaxLength(20)] public List<ItemPedidoDto> Items { get; set; } = new();
}

public class EstadoDto
{
    [Required] public string Estado { get; set; } = "";
}
