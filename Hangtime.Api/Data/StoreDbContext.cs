using Hangtime.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Hangtime.Api.Data;

/// <summary>Conexión a MySQL y mapeo de las tablas.</summary>
public class StoreDbContext : DbContext
{
    public StoreDbContext(DbContextOptions<StoreDbContext> opciones) : base(opciones) { }

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Producto> Productos => Set<Producto>();
    public DbSet<Talle> Talles => Set<Talle>();
    public DbSet<Pedido> Pedidos => Set<Pedido>();
    public DbSet<ItemPedido> Items => Set<ItemPedido>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.Entity<Usuario>(e =>
        {
            e.HasIndex(u => u.Email).IsUnique();
            e.Property(u => u.Nombre).HasMaxLength(100);
            e.Property(u => u.Email).HasMaxLength(160);
            e.Property(u => u.PasswordHash).HasMaxLength(100);
            e.Property(u => u.Rol).HasMaxLength(20);
        });

        mb.Entity<Producto>(e =>
        {
            e.Property(p => p.Nombre).HasMaxLength(120);
            e.Property(p => p.Descripcion).HasMaxLength(500);
            e.Property(p => p.Precio).HasPrecision(10, 2);
            e.Property(p => p.Categoria).HasMaxLength(30);
            e.Property(p => p.Color1).HasMaxLength(7);
            e.Property(p => p.Color2).HasMaxLength(7);
            e.Property(p => p.Color3).HasMaxLength(7);
            e.HasIndex(p => p.Categoria);
        });

        mb.Entity<Talle>(e =>
        {
            e.HasIndex(t => new { t.ProductoId, t.Numero }).IsUnique();
            e.HasOne(t => t.Producto).WithMany(p => p.Talles)
                .HasForeignKey(t => t.ProductoId).OnDelete(DeleteBehavior.Cascade);
        });

        mb.Entity<Pedido>(e =>
        {
            e.Property(p => p.Estado).HasMaxLength(20);
            e.Property(p => p.Direccion).HasMaxLength(250);
            e.Property(p => p.Total).HasPrecision(12, 2);
            e.HasOne(p => p.Usuario).WithMany(u => u.Pedidos)
                .HasForeignKey(p => p.UsuarioId).OnDelete(DeleteBehavior.Restrict);
        });

        mb.Entity<ItemPedido>(e =>
        {
            e.Property(i => i.PrecioUnitario).HasPrecision(10, 2);
            e.HasOne(i => i.Pedido).WithMany(p => p.Items)
                .HasForeignKey(i => i.PedidoId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(i => i.Talle).WithMany()
                .HasForeignKey(i => i.TalleId).OnDelete(DeleteBehavior.Restrict);
        });
    }
}
