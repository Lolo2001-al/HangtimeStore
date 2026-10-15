# 👟 HANGTIME — tienda de zapatillas retro

Tienda online hecha con **ASP.NET Core (C#, API REST)**, **Entity Framework Core 10**, **MySQL 8+**, **JWT + BCrypt** y un front minimalista animado (HTML/CSS/JS sin librerías).
Misma base que APIzza, con autenticación, roles, stock por talle y panel de administración.

## Estructura
```text
Hangtime.Api/
├── Program.cs            # JWT, rate limiting, Swagger, cabeceras de seguridad
├── Models/               # Usuario, Producto, Talle, Pedido, ItemPedido
├── Dtos/                 # validaciones de entrada
├── Data/                 # StoreDbContext + SeedData
├── Services/             # TokenService (JWT)
├── Controllers/          # Auth, Productos, Pedidos, Admin
└── wwwroot/              # front (index.html, css, js)
database/                 # schema.sql + DER (Mermaid)
```

## Cómo correrlo
1. Tener MySQL 8+ iniciado.
2. En `Hangtime.Api/appsettings.json` poné tu usuario y contraseña de MySQL.
3. Cambiá la clave JWT (no la subas a GitHub):
   `cd Hangtime.Api && dotnet user-secrets init && dotnet user-secrets set "Jwt:Key" "una-clave-larga-de-32+-caracteres"`
4. `dotnet run` → la app crea las tablas y carga 8 modelos de ejemplo.
5. Abrí la URL que muestre la consola. API documentada en `/swagger`.

**Admin de prueba:** `admin@hangtime.com` / `Admin#Hangtime2026` (cambiala en `appsettings.json` → `Seed:AdminPassword`).

## Endpoints
| Método | Ruta | Acceso |
|---|---|---|
| POST | `/api/auth/registro`, `/api/auth/login` | Público (limitado a 10/min) |
| GET | `/api/productos` (filtros `q`, `categoria`, `talle`, `max`, `orden`), `/api/productos/{id}` | Público |
| POST/PUT/DELETE | `/api/productos` | Admin |
| POST | `/api/pedidos` | Cliente |
| GET | `/api/pedidos/mios` | Cliente |
| GET / PATCH | `/api/pedidos`, `/api/pedidos/{id}/estado` | Admin |
| GET | `/api/admin/resumen` | Admin |

## Seguridad aplicada (ISO/IEC 27001)
- Contraseñas con **BCrypt** (costo 12), política de complejidad y bloqueo de 10 min tras 5 intentos fallidos.
- **JWT** firmado con expiración, roles `Cliente`/`Admin` y mensajes de error que no revelan si un email existe.
- **Anti SQL injection:** EF Core y consultas parametrizadas. **Anti XSS:** escape de salida en el front, colores validados por regex y **CSP** estricta.
- **Rate limiting** en login/registro, cabeceras `nosniff`, `X-Frame-Options`, `Referrer-Policy`.
- El **precio y el total siempre salen de la base**, nunca del cliente.

## Calidad (ISO/IEC 25010)
- **Adecuación funcional:** stock por talle con transacción y bloqueo de fila (`FOR UPDATE`); cancelar un pedido devuelve el stock.
- **Usabilidad:** responsivo, navegable por teclado, respeta `prefers-reduced-motion`.
- **Mantenibilidad:** capas separadas (Models/Dtos/Data/Services/Controllers) y código comentado.
