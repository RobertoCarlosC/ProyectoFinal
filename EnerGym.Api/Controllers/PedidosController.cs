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
    }
}
