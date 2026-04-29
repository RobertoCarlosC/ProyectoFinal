using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using EnerGym.Models;

namespace EnerGym.Controllers
{
    [ApiController]
    [Route("api/admin")]
    public class AdminController : ControllerBase
    {
        private readonly Database _db;

        public AdminController(Database db)
        {
            _db = db;
        }

        
        private bool EsAdmin(SqlConnection conn, int idUsuario)
        {
            var cmd = new SqlCommand(
                "SELECT IdRol FROM Usuarios WHERE IdUsuario = @IdUsuario",
                conn);
            cmd.Parameters.AddWithValue("@IdUsuario", idUsuario);
            var result = cmd.ExecuteScalar();
            return result != null && (int)result == 1;
        }

        
        [HttpGet("{idAdmin:int}/productos/todos")]
        public IActionResult GetProductosTodos(int idAdmin)
        {
            try
            {
                using var conn = _db.GetConnection();
                conn.Open();

                if (!EsAdmin(conn, idAdmin))
                    return Unauthorized(new { error = "No tienes permisos de administrador." });

                var cmd = new SqlCommand(@"
                    SELECT p.IdProducto, p.Nombre, p.Descripcion, p.Precio, p.Stock, p.Imagen, 
                           p.IdCategoria, c.Nombre AS NombreCategoria, COUNT(l.IdLike) AS Likes
                    FROM Productos p
                    LEFT JOIN Categorias c ON p.IdCategoria = c.IdCategoria
                    LEFT JOIN LikesProductos l ON p.IdProducto = l.IdProducto
                    GROUP BY p.IdProducto, p.Nombre, p.Descripcion, p.Precio, p.Stock, p.Imagen, 
                             p.IdCategoria, c.Nombre
                    ORDER BY p.IdProducto DESC",
                    conn);

                var productos = new List<object>();
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    productos.Add(new
                    {
                        idProducto = (int)reader["IdProducto"],
                        nombre = reader["Nombre"].ToString(),
                        descripcion = reader["Descripcion"] == DBNull.Value ? "" : reader["Descripcion"].ToString(),
                        precio = Convert.ToDecimal(reader["Precio"]),
                        stock = (int)reader["Stock"],
                        imagen = reader["Imagen"] == DBNull.Value ? "" : reader["Imagen"].ToString(),
                        idCategoria = (int)reader["IdCategoria"],
                        nombreCategoria = reader["NombreCategoria"] == DBNull.Value ? "" : reader["NombreCategoria"].ToString(),
                        likes = (int)reader["Likes"]
                    });
                }

                return Ok(productos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        
        [HttpPost("{idAdmin:int}/productos/crear")]
        public IActionResult CrearProducto(int idAdmin, [FromBody] CrearProductoAdminDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Nombre) || dto.Precio <= 0 || dto.Stock < 0 || dto.IdCategoria <= 0)
                return BadRequest(new { error = "Campos inválidos. Nombre, Precio, Categoría son obligatorios." });

            try
            {
                using var conn = _db.GetConnection();
                conn.Open();

                if (!EsAdmin(conn, idAdmin))
                    return Unauthorized(new { error = "No tienes permisos de administrador." });

                var cmd = new SqlCommand(@"
                    INSERT INTO Productos (Nombre, Descripcion, Precio, Stock, Imagen, IdCategoria)
                    OUTPUT INSERTED.IdProducto
                    VALUES (@Nombre, @Descripcion, @Precio, @Stock, @Imagen, @IdCategoria)",
                    conn);

                cmd.Parameters.AddWithValue("@Nombre", dto.Nombre.Trim());
                cmd.Parameters.AddWithValue("@Descripcion", (object?)dto.Descripcion ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Precio", dto.Precio);
                cmd.Parameters.AddWithValue("@Stock", dto.Stock);
                cmd.Parameters.AddWithValue("@Imagen", (object?)dto.Imagen ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@IdCategoria", dto.IdCategoria);

                int idProducto = (int)cmd.ExecuteScalar()!;
                return Created("", new { idProducto, message = "Producto creado correctamente." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        
        [HttpPut("{idAdmin:int}/productos/{idProducto:int}/editar")]
        public IActionResult EditarProducto(int idAdmin, int idProducto, [FromBody] EditarProductoAdminDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Nombre) || dto.Precio <= 0 || dto.Stock < 0 || dto.IdCategoria <= 0)
                return BadRequest(new { error = "Campos inválidos." });

            try
            {
                using var conn = _db.GetConnection();
                conn.Open();

                if (!EsAdmin(conn, idAdmin))
                    return Unauthorized(new { error = "No tienes permisos de administrador." });

                var cmd = new SqlCommand(@"
                    UPDATE Productos
                    SET Nombre = @Nombre, Descripcion = @Descripcion, Precio = @Precio, 
                        Stock = @Stock, Imagen = @Imagen, IdCategoria = @IdCategoria
                    WHERE IdProducto = @IdProducto",
                    conn);

                cmd.Parameters.AddWithValue("@Nombre", dto.Nombre.Trim());
                cmd.Parameters.AddWithValue("@Descripcion", (object?)dto.Descripcion ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Precio", dto.Precio);
                cmd.Parameters.AddWithValue("@Stock", dto.Stock);
                cmd.Parameters.AddWithValue("@Imagen", (object?)dto.Imagen ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@IdCategoria", dto.IdCategoria);
                cmd.Parameters.AddWithValue("@IdProducto", idProducto);

                int filas = cmd.ExecuteNonQuery();
                if (filas == 0)
                    return NotFound(new { error = "Producto no encontrado." });

                return Ok(new { message = "Producto actualizado correctamente." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        
        [HttpDelete("{idAdmin:int}/productos/{idProducto:int}")]
        public IActionResult EliminarProducto(int idAdmin, int idProducto)
        {
            try
            {
                using var conn = _db.GetConnection();
                conn.Open();

                if (!EsAdmin(conn, idAdmin))
                    return Unauthorized(new { error = "No tienes permisos de administrador." });

                
                var cmd = new SqlCommand("DELETE FROM Productos WHERE IdProducto = @IdProducto", conn);
                cmd.Parameters.AddWithValue("@IdProducto", idProducto);
                int filas = cmd.ExecuteNonQuery();

                if (filas == 0)
                    return NotFound(new { error = "Producto no encontrado." });

                return Ok(new { message = "Producto eliminado correctamente." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        
        [HttpGet("{idAdmin:int}/usuarios/todos")]
        public IActionResult GetUsuariosTodos(int idAdmin)
        {
            try
            {
                using var conn = _db.GetConnection();
                conn.Open();

                if (!EsAdmin(conn, idAdmin))
                    return Unauthorized(new { error = "No tienes permisos de administrador." });

                var cmd = new SqlCommand(@"
                    SELECT u.IdUsuario, u.Nombre, u.Email, u.FechaRegistro, u.IdRol
                    FROM Usuarios u
                    ORDER BY u.FechaRegistro DESC",
                    conn);

                var usuarios = new List<object>();
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    usuarios.Add(new
                    {
                        idUsuario = (int)reader["IdUsuario"],
                        nombre = reader["Nombre"].ToString(),
                        email = reader["Email"].ToString(),
                        telefono = "",
                        direccion = "",
                        ciudad = "",
                        codigoPostal = "",
                        fechaRegistro = reader["FechaRegistro"],
                        rol = (int)reader["IdRol"] == 1 ? "admin" : "usuario",
                        idRol = (int)reader["IdRol"]
                    });
                }

                return Ok(usuarios);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        
        [HttpPut("{idAdmin:int}/usuarios/{idUsuario:int}/editar")]
        public IActionResult EditarUsuario(int idAdmin, int idUsuario, [FromBody] EditarUsuarioAdminDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Nombre) || string.IsNullOrWhiteSpace(dto.Email))
                return BadRequest(new { error = "Nombre y Email son obligatorios." });

            try
            {
                using var conn = _db.GetConnection();
                conn.Open();

                if (!EsAdmin(conn, idAdmin))
                    return Unauthorized(new { error = "No tienes permisos de administrador." });

                var cmd = new SqlCommand(@"
                    UPDATE Usuarios
                    SET Nombre = @Nombre, Email = @Email, IdRol = @IdRol
                    WHERE IdUsuario = @IdUsuario",
                    conn);

                cmd.Parameters.AddWithValue("@Nombre", dto.Nombre.Trim());
                cmd.Parameters.AddWithValue("@Email", dto.Email.Trim().ToLower());
                cmd.Parameters.AddWithValue("@IdRol", dto.IdRol);
                cmd.Parameters.AddWithValue("@IdUsuario", idUsuario);

                int filas = cmd.ExecuteNonQuery();
                if (filas == 0)
                    return NotFound(new { error = "Usuario no encontrado." });

                return Ok(new { message = "Usuario actualizado correctamente." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        
        [HttpDelete("{idAdmin:int}/usuarios/{idUsuario:int}")]
        public IActionResult EliminarUsuario(int idAdmin, int idUsuario)
        {
            if (idAdmin == idUsuario)
                return BadRequest(new { error = "No puedes eliminar tu propia cuenta." });

            try
            {
                using var conn = _db.GetConnection();
                conn.Open();

                if (!EsAdmin(conn, idAdmin))
                    return Unauthorized(new { error = "No tienes permisos de administrador." });

                var cmd = new SqlCommand("DELETE FROM Usuarios WHERE IdUsuario = @IdUsuario", conn);
                cmd.Parameters.AddWithValue("@IdUsuario", idUsuario);
                int filas = cmd.ExecuteNonQuery();

                if (filas == 0)
                    return NotFound(new { error = "Usuario no encontrado." });

                return Ok(new { message = "Usuario eliminado correctamente." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        
        [HttpGet("{idAdmin:int}/usuarios/{idUsuario:int}/carrito")]
        public IActionResult GetCarritoUsuario(int idAdmin, int idUsuario)
        {
            try
            {
                using var conn = _db.GetConnection();
                conn.Open();

                if (!EsAdmin(conn, idAdmin))
                    return Unauthorized(new { error = "No tienes permisos de administrador." });

                var carritoCmd = new SqlCommand(
                    "SELECT IdCarrito FROM Carritos WHERE IdUsuario = @IdUsuario",
                    conn);
                carritoCmd.Parameters.AddWithValue("@IdUsuario", idUsuario);
                var carritoResult = carritoCmd.ExecuteScalar();
if (carritoResult == null || carritoResult == DBNull.Value)
    return Ok(new { carrito = (int?)null, items = new List<object>(), total = 0 });

                int idCarrito = (int)carritoResult;

                var itemsCmd = new SqlCommand(@"
                    SELECT cp.Id, cp.IdProducto, cp.Cantidad, p.Nombre, p.Precio, p.Imagen
                    FROM CarritoProductos cp
                    INNER JOIN Productos p ON cp.IdProducto = p.IdProducto
                    WHERE cp.IdCarrito = @IdCarrito",
                    conn);
                itemsCmd.Parameters.AddWithValue("@IdCarrito", idCarrito);

                var items = new List<object>();
                decimal total = 0;
                using var reader = itemsCmd.ExecuteReader();
                while (reader.Read())
                {
                    decimal precio = Convert.ToDecimal(reader["Precio"]);
                    int cantidad = (int)reader["Cantidad"];
                    decimal subtotal = precio * cantidad;
                    total += subtotal;

                    items.Add(new
                    {
                        id = (int)reader["Id"],
                        idProducto = (int)reader["IdProducto"],
                        nombre = reader["Nombre"].ToString(),
                        precio = precio,
                        cantidad = cantidad,
                        subtotal = subtotal
                    });
                }

                return Ok(new { idCarrito, items, total });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        
        [HttpGet("{idAdmin:int}/pedidos/todos")]
        public IActionResult GetPedidosTodos(int idAdmin)
        {
            try
            {
                using var conn = _db.GetConnection();
                conn.Open();

                if (!EsAdmin(conn, idAdmin))
                    return Unauthorized(new { error = "No tienes permisos de administrador." });

                var cmd = new SqlCommand(@"
                    SELECT p.IdPedido, p.IdUsuario, p.Fecha, p.Total, p.Estado, u.Nombre AS NombreUsuario
                    FROM Pedidos p
                    INNER JOIN Usuarios u ON p.IdUsuario = u.IdUsuario
                    ORDER BY p.Fecha DESC",
                    conn);

                var pedidos = new List<object>();
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    pedidos.Add(new
                    {
                        idPedido = (int)reader["IdPedido"],
                        idUsuario = (int)reader["IdUsuario"],
                        nombreUsuario = reader["NombreUsuario"].ToString(),
                        fecha = reader["Fecha"],
                        total = Convert.ToDecimal(reader["Total"]),
                        estado = reader["Estado"].ToString()
                    });
                }

                return Ok(pedidos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        
        [HttpPut("{idAdmin:int}/pedidos/{idPedido:int}/cambiar-estado")]
        public IActionResult CambiarEstadoPedido(int idAdmin, int idPedido, [FromBody] CambiarEstadoPedidoDto dto)
        {
            var estadosValidos = new[] { "Pendiente", "Preparando pedido", "Enviado", "En reparto", "Entregado" };
            if (!estadosValidos.Contains(dto.NuevoEstado))
                return BadRequest(new { error = "Estado inválido. Estados válidos: Pendiente, Preparando pedido, Enviado, En reparto, Entregado" });

            try
            {
                using var conn = _db.GetConnection();
                conn.Open();

                if (!EsAdmin(conn, idAdmin))
                    return Unauthorized(new { error = "No tienes permisos de administrador." });

                var cmd = new SqlCommand(
                    "UPDATE Pedidos SET Estado = @Estado WHERE IdPedido = @IdPedido",
                    conn);
                cmd.Parameters.AddWithValue("@Estado", dto.NuevoEstado);
                cmd.Parameters.AddWithValue("@IdPedido", idPedido);

                int filas = cmd.ExecuteNonQuery();
                if (filas == 0)
                    return NotFound(new { error = "Pedido no encontrado." });

                return Ok(new { message = $"Estado del pedido cambiado a '{dto.NuevoEstado}'." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        
        [HttpGet("{idAdmin:int}/pedidos/{idPedido:int}/detalles")]
        public IActionResult GetDetallesPedido(int idAdmin, int idPedido)
        {
            try
            {
                using var conn = _db.GetConnection();
                conn.Open();

                if (!EsAdmin(conn, idAdmin))
                    return Unauthorized(new { error = "No tienes permisos de administrador." });

                var pedidoCmd = new SqlCommand(@"
                    SELECT p.IdPedido, p.IdUsuario, p.Fecha, p.Total, p.Estado, u.Nombre
                    FROM Pedidos p
                    INNER JOIN Usuarios u ON p.IdUsuario = u.IdUsuario
                    WHERE p.IdPedido = @IdPedido",
                    conn);
                pedidoCmd.Parameters.AddWithValue("@IdPedido", idPedido);

                object? pedido = null;
                using (var reader = pedidoCmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        pedido = new
                        {
                            idPedido = (int)reader["IdPedido"],
                            idUsuario = (int)reader["IdUsuario"],
                            nombreUsuario = reader["Nombre"].ToString(),
                            fecha = reader["Fecha"],
                            total = Convert.ToDecimal(reader["Total"]),
                            estado = reader["Estado"].ToString()
                        };
                    }
                }

                if (pedido == null)
                    return NotFound(new { error = "Pedido no encontrado." });

                var detallesCmd = new SqlCommand(@"
                    SELECT pp.IdDetalle, pp.IdProducto, pp.Cantidad, pp.Precio, p.Nombre, p.Imagen
                    FROM PedidoProductos pp
                    INNER JOIN Productos p ON pp.IdProducto = p.IdProducto
                    WHERE pp.IdPedido = @IdPedido",
                    conn);
                detallesCmd.Parameters.AddWithValue("@IdPedido", idPedido);

                var detalles = new List<object>();
                using (var reader = detallesCmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        detalles.Add(new
                        {
                            idDetalle = (int)reader["IdDetalle"],
                            idProducto = (int)reader["IdProducto"],
                            nombre = reader["Nombre"].ToString(),
                            imagen = reader["Imagen"] == DBNull.Value ? "" : reader["Imagen"].ToString(),
                            cantidad = (int)reader["Cantidad"],
                            precio = Convert.ToDecimal(reader["Precio"]),
                            subtotal = (int)reader["Cantidad"] * Convert.ToDecimal(reader["Precio"])
                        });
                    }
                }

                return Ok(new { pedido, detalles });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        
        [HttpGet("{idAdmin:int}/estadisticas")]
        public IActionResult GetEstadisticas(int idAdmin)
        {
            try
            {
                using var conn = _db.GetConnection();
                conn.Open();

                if (!EsAdmin(conn, idAdmin))
                    return Unauthorized(new { error = "No tienes permisos de administrador." });

                
                var usuariosCmd = new SqlCommand("SELECT COUNT(*) FROM Usuarios WHERE IdRol = 2", conn);
                int totalUsuarios = (int)usuariosCmd.ExecuteScalar()!;

                
                var pedidosCmd = new SqlCommand("SELECT COUNT(*) FROM Pedidos", conn);
                int totalPedidos = (int)pedidosCmd.ExecuteScalar()!;

                
                var ingresosCmd = new SqlCommand("SELECT ISNULL(SUM(Total), 0) FROM Pedidos", conn);
                decimal ingresosTotales = Convert.ToDecimal(ingresosCmd.ExecuteScalar()!);

                
                var productosCmd = new SqlCommand("SELECT COUNT(*) FROM Productos", conn);
                int totalProductos = (int)productosCmd.ExecuteScalar()!;

                
                var sinStockCmd = new SqlCommand("SELECT COUNT(*) FROM Productos WHERE Stock = 0", conn);
                int productosSinStock = (int)sinStockCmd.ExecuteScalar()!;

                
                var estadosCmd = new SqlCommand(@"
                    SELECT Estado, COUNT(*) as Cantidad
                    FROM Pedidos
                    GROUP BY Estado",
                    conn);

                var estadosDistribucion = new List<object>();
                using (var reader = estadosCmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        estadosDistribucion.Add(new
                        {
                            estado = reader["Estado"].ToString(),
                            cantidad = (int)reader["Cantidad"]
                        });
                    }
                }

                return Ok(new
                {
                    totalUsuarios,
                    totalPedidos,
                    ingresosTotales,
                    totalProductos,
                    productosSinStock,
                    pedidosPorEstado = estadosDistribucion
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
