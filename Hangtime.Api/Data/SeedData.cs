using Hangtime.Api.Models;

namespace Hangtime.Api.Data;

public static class SeedData
{
    /// <summary>Crea las tablas si faltan y carga el catálogo y el usuario admin la primera vez.</summary>
    public static void Inicializar(StoreDbContext db, IConfiguration config)
    {
        db.Database.EnsureCreated();

        if (!db.Usuarios.Any())
        {
            db.Usuarios.Add(new Usuario
            {
                Nombre = "Administrador",
                Email = "admin@hangtime.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(config["Seed:AdminPassword"] ?? "Admin#Hangtime2026", 12),
                Rol = "Admin"
            });
        }

        if (!db.Productos.Any())
        {
            var rnd = new Random(7);
            Producto P(string n, string d, decimal precio, string cat, string c1, string c2, string c3, bool dest) => new()
            {
                Nombre = n, Descripcion = d, Precio = precio, Categoria = cat,
                Color1 = c1, Color2 = c2, Color3 = c3, Destacado = dest,
                Talles = Enumerable.Range(38, 9).Select(t => new Talle { Numero = t, Stock = rnd.Next(0, 7) }).ToList()
            };

            db.Productos.AddRange(
                P("Retro 1 High · Chicago", "Silueta alta clásica, cuero y paneles bicolor.", 289000, "high", "#f6f5f2", "#d6001c", "#111111", true),
                P("Retro 1 Low · Shadow", "Perfil bajo en gris y negro, uso diario.", 245000, "low", "#6b6b6b", "#111111", "#f6f5f2", false),
                P("Retro 3 · White Cement", "Estampa elefante sobre base blanca.", 310000, "mid", "#f6f5f2", "#8a8a8a", "#d6001c", true),
                P("Retro 4 · Bred", "Malla lateral y detalles en rojo intenso.", 335000, "mid", "#111111", "#d6001c", "#f6f5f2", true),
                P("Retro 5 · Fire Red", "Lengüeta reflectiva y suela translúcida.", 320000, "mid", "#f6f5f2", "#d6001c", "#111111", false),
                P("Retro 6 · Infrared", "Negro profundo con acento infrarrojo.", 330000, "mid", "#111111", "#ff4d5e", "#f6f5f2", false),
                P("Retro 11 · Concord", "Charol negro y suela cristal.", 389000, "high", "#111111", "#f6f5f2", "#3b2f7f", true),
                P("Retro 12 · Taxi", "Costuras marcadas, toque amarillo.", 342000, "mid", "#f6f5f2", "#f2b705", "#111111", false));
        }

        db.SaveChanges();
    }
}
