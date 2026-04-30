using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using EnerGym.Models;

namespace EnerGym.Controllers
{
    [ApiController]
    [Route("api/pedidos")]
    public class PedidosController : ControllerBase
    {
        private readonly Database _db;

        public PedidosController(Database db)
        {
            _db = db;
        }

        
        [HttpPost("confirmar")]
        public IActionResult ConfirmarPedido([FromBody] ConfirmarPedidoDto dto)
        {
            try
            {
                using var conn = _db.GetConnection();
                conn.Open();

                using var transaction = conn.BeginTransaction();

                try
                {
                    var carritoCmd = new SqlCommand(
                        "SELECT IdCarrito FROM Carritos WHERE IdUsuario = @IdUsuario",
                        conn, transaction);
                    carritoCmd.Parameters.AddWithValue("@IdUsuario", dto.IdUsuario);
                    var carritoResult = carritoCmd.ExecuteScalar();

                    if (carritoResult == null || carritoResult == DBNull.Value)
                        return BadRequest(new { error = "No tienes un carrito activo." });

                    int idCarrito = (int)carritoResult;

                    var itemsCmd = new SqlCommand(@"
                        SELECT cp.IdProducto, cp.Cantidad, p.Precio, p.Stock, p.Nombre
                        FROM CarritoProductos cp
                        INNER JOIN Productos p ON cp.IdProducto = p.IdProducto
                        WHERE cp.IdCarrito = @IdCarrito",
                        conn, transaction);
                    itemsCmd.Parameters.AddWithValue("@IdCarrito", idCarrito);

                    var items = new List<(int idProducto, int cantidad, decimal precio, int stock, string nombre)>();
                    using (var reader = itemsCmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            items.Add((
                                (int)reader["IdProducto"],
                                (int)reader["Cantidad"],
                                Convert.ToDecimal(reader["Precio"]),
                                (int)reader["Stock"],
                                reader["Nombre"].ToString()!
                            ));
                        }
                    }

                    if (items.Count == 0)
                        return BadRequest(new { error = "El carrito está vacío." });

                    foreach (var item in items)
                    {
                        if (item.stock < item.cantidad)
                            return BadRequest(new
                            {
                                error = $"Stock insuficiente para '{item.nombre}'. Disponible: {item.stock}, solicitado: {item.cantidad}."
                            });
                    }

                    decimal total = items.Sum(i => i.precio * i.cantidad);

                    var pedidoCmd = new SqlCommand(
                        "INSERT INTO Pedidos (IdUsuario, Fecha, Total, Estado, DireccionEnvio, MetodoPago) OUTPUT INSERTED.IdPedido VALUES (@IdUsuario, GETDATE(), @Total, 'Pendiente', @DireccionEnvio, @MetodoPago)",
                        conn, transaction);
                    pedidoCmd.Parameters.AddWithValue("@IdUsuario", dto.IdUsuario);
                    pedidoCmd.Parameters.AddWithValue("@Total", total);
                    pedidoCmd.Parameters.AddWithValue("@DireccionEnvio", (object?)dto.DireccionEnvio ?? DBNull.Value);
                    pedidoCmd.Parameters.AddWithValue("@MetodoPago", (object?)dto.MetodoPago ?? DBNull.Value);
                    int idPedido = (int)pedidoCmd.ExecuteScalar()!;

                    foreach (var item in items)
                    {
                        var detalleCmd = new SqlCommand(
                            @"INSERT INTO PedidoProductos (IdPedido, IdProducto, Cantidad, Precio)
                              VALUES (@IdPedido, @IdProducto, @Cantidad, @Precio)",
                            conn, transaction);
                        detalleCmd.Parameters.AddWithValue("@IdPedido",   idPedido);
                        detalleCmd.Parameters.AddWithValue("@IdProducto", item.idProducto);
                        detalleCmd.Parameters.AddWithValue("@Cantidad",   item.cantidad);
                        detalleCmd.Parameters.AddWithValue("@Precio",     item.precio);
                        detalleCmd.ExecuteNonQuery();

                        var stockCmd = new SqlCommand(
                            "UPDATE Productos SET Stock = Stock - @Cantidad WHERE IdProducto = @IdProducto",
                            conn, transaction);
                        stockCmd.Parameters.AddWithValue("@Cantidad",   item.cantidad);
                        stockCmd.Parameters.AddWithValue("@IdProducto", item.idProducto);
                        stockCmd.ExecuteNonQuery();
                    }

                    var clearCmd = new SqlCommand(
                        "DELETE FROM CarritoProductos WHERE IdCarrito = @IdCarrito",
                        conn, transaction);
                    clearCmd.Parameters.AddWithValue("@IdCarrito", idCarrito);
                    clearCmd.ExecuteNonQuery();

                    transaction.Commit();
                    return Ok(new { message = "Pedido confirmado correctamente.", idPedido, total });
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        
        [HttpGet("detalle/{idPedido:int}")]
        public IActionResult GetDetallePedido(int idPedido)
        {
            try
            {
                using var conn = _db.GetConnection();
                conn.Open();

                // Datos del pedido
                var pedidoCmd = new SqlCommand(
                    "SELECT IdPedido, Fecha, Total, Estado, DireccionEnvio, MetodoPago FROM Pedidos WHERE IdPedido = @IdPedido",
                    conn);
                pedidoCmd.Parameters.AddWithValue("@IdPedido", idPedido);

                object? pedidoRow = null;
                using (var r = pedidoCmd.ExecuteReader())
                {
                    if (r.Read())
                    {
                        pedidoRow = new
                        {
                            idPedido = (int)r["IdPedido"],
                            fecha = r["Fecha"],
                            total = Convert.ToDecimal(r["Total"]),
                            estado = r["Estado"].ToString(),
                            direccionEnvio = r["DireccionEnvio"]?.ToString(),
                            metodoPago = r["MetodoPago"]?.ToString()
                        };
                    }
                }

                if (pedidoRow == null)
                    return NotFound(new { error = "Pedido no encontrado" });

                // Productos del pedido con imagen principal
                var cmd = new SqlCommand(@"
                    SELECT pp.IdDetalle, pp.IdProducto, pp.Cantidad, pp.Precio,
                           p.Nombre AS NombreProducto, p.Imagen AS ImagenProducto
                    FROM PedidoProductos pp
                    INNER JOIN Productos p ON pp.IdProducto = p.IdProducto
                    WHERE pp.IdPedido = @IdPedido",
                    conn);
                cmd.Parameters.AddWithValue("@IdPedido", idPedido);

                var detalles = new List<object>();
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    detalles.Add(new
                    {
                        idDetalle = (int)reader["IdDetalle"],
                        idProducto = (int)reader["IdProducto"],
                        nombreProducto = reader["NombreProducto"].ToString(),
                        cantidad = (int)reader["Cantidad"],
                        precio = Convert.ToDecimal(reader["Precio"]),
                        imagen = reader["ImagenProducto"]?.ToString() ?? ""
                    });
                }

                return Ok(new { pedido = pedidoRow, productos = detalles });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        
        [HttpGet("{idUsuario:int}")]
        public IActionResult GetPedidos(int idUsuario)
        {
            try
            {
                using var conn = _db.GetConnection();
                conn.Open();

                var cmd = new SqlCommand(
                    "SELECT IdPedido, Fecha, Total, Estado FROM Pedidos WHERE IdUsuario = @IdUsuario ORDER BY Fecha DESC",
                    conn);
                cmd.Parameters.AddWithValue("@IdUsuario", idUsuario);

                var pedidos = new List<object>();
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    pedidos.Add(new
                    {
                        idPedido = (int)reader["IdPedido"],
                        fecha    = reader["Fecha"],
                        total    = Convert.ToDecimal(reader["Total"]),
                        estado   = reader["Estado"].ToString()
                    });
                }

                return Ok(pedidos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // ========== APIs PARA ADMIN ==========

        [HttpGet("admin/todos")]
        public IActionResult GetAllPedidos([FromQuery] string? estado = null, [FromQuery] int? idUsuario = null)
        {
            try
            {
                using var conn = _db.GetConnection();
                conn.Open();

                string query = @"
                    SELECT p.IdPedido, p.IdUsuario, u.Nombre AS UsuarioNombre, u.Email, p.Fecha, p.Total, 
                           p.Estado, p.DireccionEnvio, 
                           COUNT(pp.IdDetalle) AS CantidadProductos
                    FROM Pedidos p
                    INNER JOIN Usuarios u ON p.IdUsuario = u.IdUsuario
                    LEFT JOIN PedidoProductos pp ON p.IdPedido = pp.IdPedido
                    WHERE 1=1";

                if (!string.IsNullOrEmpty(estado))
                    query += " AND p.Estado = @Estado";
                if (idUsuario.HasValue)
                    query += " AND p.IdUsuario = @IdUsuario";

                query += " GROUP BY p.IdPedido, p.IdUsuario, u.Nombre, u.Email, p.Fecha, p.Total, p.Estado, p.DireccionEnvio ORDER BY p.Fecha DESC";

                var cmd = new SqlCommand(query, conn);
                if (!string.IsNullOrEmpty(estado))
                    cmd.Parameters.AddWithValue("@Estado", estado);
                if (idUsuario.HasValue)
                    cmd.Parameters.AddWithValue("@IdUsuario", idUsuario.Value);

                var pedidos = new List<object>();
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    pedidos.Add(new
                    {
                        idPedido = (int)reader["IdPedido"],
                        idUsuario = (int)reader["IdUsuario"],
                        usuarioNombre = reader["UsuarioNombre"].ToString(),
                        usuarioEmail = reader["Email"].ToString(),
                        fecha = reader["Fecha"],
                        total = Convert.ToDecimal(reader["Total"]),
                        estado = reader["Estado"].ToString(),
                        direccionEnvio = reader["DireccionEnvio"]?.ToString(),
                        cantidadProductos = (int)reader["CantidadProductos"]
                    });
                }

                return Ok(pedidos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("admin/estadisticas")]
        public IActionResult GetEstadisticas()
        {
            try
            {
                using var conn = _db.GetConnection();
                conn.Open();

                // Query para obtener estadísticas
                var cmd = new SqlCommand(@"
                    SELECT 
                        COUNT(*) as TotalPedidos,
                        SUM(CASE WHEN Estado = 'Pendiente' THEN 1 ELSE 0 END) as Pendientes,
                        SUM(CASE WHEN Estado = 'Procesando' THEN 1 ELSE 0 END) as EnProceso,
                        SUM(CASE WHEN Estado = 'Enviado' OR Estado = 'En reparto' THEN 1 ELSE 0 END) as Enviados,
                        SUM(CASE WHEN Estado = 'Entregado' THEN 1 ELSE 0 END) as Entregados,
                        SUM(Total) as VentasTotal
                    FROM Pedidos", conn);

                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    int total = reader.IsDBNull(0) ? 0 : (int)reader["TotalPedidos"];
                    decimal suma = reader.IsDBNull(5) ? 0 : Convert.ToDecimal(reader["VentasTotal"]);

                    return Ok(new
                    {
                        totalPedidos = total,
                        pendientesConfirmacion = reader.IsDBNull(1) ? 0 : (int)reader["Pendientes"],
                        enProceso = reader.IsDBNull(2) ? 0 : (int)reader["EnProceso"],
                        enviados = reader.IsDBNull(3) ? 0 : (int)reader["Enviados"],
                        entregados = reader.IsDBNull(4) ? 0 : (int)reader["Entregados"],
                        ventasTotal = suma,
                        promedioVenta = total > 0 ? suma / total : 0
                    });
                }

                return Ok(new { error = "No se pudieron obtener estadísticas" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("admin/{idPedido:int}/estado")]
        public IActionResult CambiarEstadoAdmin(int idPedido, [FromBody] CambiarEstadoPedidoAdminDto dto)
        {
            try
            {
                if (idPedido != dto.IdPedido)
                    return BadRequest(new { error = "El ID del pedido no coincide" });

                // Validar que sea un estado válido
                var estadosValidos = new[] { "Pendiente", "Procesando", "Enviado", "En reparto", "Entregado" };
                if (!estadosValidos.Contains(dto.NuevoEstado))
                    return BadRequest(new { error = "Estado inválido" });

                using var conn = _db.GetConnection();
                conn.Open();

                // Obtener estado actual
                var getCurrentCmd = new SqlCommand("SELECT Estado FROM Pedidos WHERE IdPedido = @IdPedido", conn);
                getCurrentCmd.Parameters.AddWithValue("@IdPedido", idPedido);
                var estadoActual = getCurrentCmd.ExecuteScalar()?.ToString();

                if (estadoActual == null)
                    return NotFound(new { error = "Pedido no encontrado" });

                // Actualizar el pedido
                var updateCmd = new SqlCommand(
                    "UPDATE Pedidos SET Estado = @Estado, FechaActualizacion = GETDATE() WHERE IdPedido = @IdPedido",
                    conn);
                updateCmd.Parameters.AddWithValue("@Estado", dto.NuevoEstado);
                updateCmd.Parameters.AddWithValue("@IdPedido", idPedido);
                updateCmd.ExecuteNonQuery();

                // Registrar en el historial
                var historialCmd = new SqlCommand(
                    @"INSERT INTO PedidoHistorialEstados (IdPedido, EstadoAnterior, EstadoNuevo, Fecha, CambiadoPor, Notas)
                      VALUES (@IdPedido, @EstadoAnterior, @EstadoNuevo, GETDATE(), @CambiadoPor, @Notas)",
                    conn);
                historialCmd.Parameters.AddWithValue("@IdPedido", idPedido);
                historialCmd.Parameters.AddWithValue("@EstadoAnterior", (object?)estadoActual ?? DBNull.Value);
                historialCmd.Parameters.AddWithValue("@EstadoNuevo", dto.NuevoEstado);
                historialCmd.Parameters.AddWithValue("@CambiadoPor", $"Admin_{dto.IdAdmin}");
                historialCmd.Parameters.AddWithValue("@Notas", (object?)dto.Notas ?? DBNull.Value);
                historialCmd.ExecuteNonQuery();

                return Ok(new { message = "Estado actualizado correctamente", idPedido, nuevoEstado = dto.NuevoEstado });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("{idPedido:int}/historial")]
        public IActionResult GetHistorialPedido(int idPedido)
        {
            try
            {
                using var conn = _db.GetConnection();
                conn.Open();

                var cmd = new SqlCommand(
                    @"SELECT IdHistorial, IdPedido, EstadoAnterior, EstadoNuevo, Fecha, CambiadoPor, Notas
                      FROM PedidoHistorialEstados
                      WHERE IdPedido = @IdPedido
                      ORDER BY Fecha DESC",
                    conn);
                cmd.Parameters.AddWithValue("@IdPedido", idPedido);

                var historial = new List<object>();
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    historial.Add(new
                    {
                        idHistorial = (int)reader["IdHistorial"],
                        idPedido = (int)reader["IdPedido"],
                        estadoAnterior = reader["EstadoAnterior"]?.ToString(),
                        estadoNuevo = reader["EstadoNuevo"].ToString(),
                        fecha = reader["Fecha"],
                        cambiadoPor = reader["CambiadoPor"]?.ToString(),
                        notas = reader["Notas"]?.ToString()
                    });
                }

                return Ok(historial);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // ========== APIs PARA USUARIO ==========

        [HttpPost("{idPedido:int}/confirmar-entrega")]
        public IActionResult ConfirmarEntrega(int idPedido, [FromBody] ConfirmarEntregaDto dto)
        {
            try
            {
                if (idPedido != dto.IdPedido)
                    return BadRequest(new { error = "El ID del pedido no coincide" });

                using var conn = _db.GetConnection();
                conn.Open();

                // Verificar que el pedido pertenezca al usuario
                var checkCmd = new SqlCommand(
                    "SELECT IdUsuario, Estado FROM Pedidos WHERE IdPedido = @IdPedido",
                    conn);
                checkCmd.Parameters.AddWithValue("@IdPedido", idPedido);
                using var reader = checkCmd.ExecuteReader();
                if (!reader.Read())
                    return NotFound(new { error = "Pedido no encontrado" });

                int idUsuarioPedido = (int)reader["IdUsuario"];
                string estadoActual = reader["Estado"].ToString() ?? "";
                reader.Close();

                if (idUsuarioPedido != dto.IdUsuario)
                    return Unauthorized(new { error = "No tienes permiso para confirmar este pedido" });

                // Validar que esté en estado "Enviado" o "En reparto"
                if (estadoActual != "Enviado" && estadoActual != "En reparto")
                    return BadRequest(new { error = "Este pedido no puede ser confirmado en estado: " + estadoActual });

                // Actualizar el pedido
                var updateCmd = new SqlCommand(
                    @"UPDATE Pedidos 
                      SET Estado = 'Entregado', 
                          FechaConfirmacionEntrega = GETDATE(),
                          FechaActualizacion = GETDATE()
                      WHERE IdPedido = @IdPedido",
                    conn);
                updateCmd.Parameters.AddWithValue("@IdPedido", idPedido);
                updateCmd.ExecuteNonQuery();

                // Registrar en historial
                var historialCmd = new SqlCommand(
                    @"INSERT INTO PedidoHistorialEstados (IdPedido, EstadoAnterior, EstadoNuevo, Fecha, CambiadoPor, Notas)
                      VALUES (@IdPedido, @EstadoAnterior, 'Entregado', GETDATE(), @CambiadoPor, @Notas)",
                    conn);
                historialCmd.Parameters.AddWithValue("@IdPedido", idPedido);
                historialCmd.Parameters.AddWithValue("@EstadoAnterior", estadoActual);
                historialCmd.Parameters.AddWithValue("@CambiadoPor", $"Usuario_{dto.IdUsuario}");
                historialCmd.Parameters.AddWithValue("@Notas", "Cliente confirmó recepción del pedido");
                historialCmd.ExecuteNonQuery();

                return Ok(new { message = "Entrega confirmada correctamente", idPedido });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
