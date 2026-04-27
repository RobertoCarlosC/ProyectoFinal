# EnerGym – Proyecto Final DAW

Tienda online de suplementos deportivos.
**Stack:** ASP.NET Core 8 + SQL Server (Somee) + HTML/CSS/JS

---

## PASO 1 – Base de datos

1. Abre **SQL Server Management Studio** o **Azure Data Studio**
2. Conéctate con estos datos:
   - Servidor: `EnerGymDB.mssql.somee.com`
   - Usuario: `robcarnav_SQLLogin_1`
   - Contraseña: `12345678`
   - BD: `EnerGymDB`
3. Abre el archivo `EnerGymDB_Setup.sql`
4. Pulsa **F5** para ejecutarlo

Esto crea todas las tablas, los roles, el usuario admin y 12 productos de ejemplo.

**Admin creado:**
- Email: `admin@energym.es`
- Password: `Admin1234`

---

## PASO 2 – Ejecutar la API

```bash
cd EnerGym
dotnet restore
dotnet run
```

Abrir en el navegador: `http://localhost:5000`

---

## Estructura

```
EnerGym/
├── Controllers/
│   ├── AuthController.cs       → /api/register  /api/login  /api/usuarios
│   ├── ProductosController.cs  → /api/productos  /api/productos/categorias
│   ├── CarritoController.cs    → /api/carrito
│   ├── PedidosController.cs    → /api/pedidos
│   └── LikesController.cs      → /api/likes
├── Models/Models.cs
├── Database.cs
├── Program.cs
├── appsettings.json
├── EnerGymDB_Setup.sql         ← ejecutar en SQL Server
└── wwwroot/
    ├── index.html
    ├── css/style.css
    ├── js/app.js
    └── pages/
        ├── login.html
        ├── registro.html
        ├── tienda.html
        ├── carrito.html
        └── admin.html
```
