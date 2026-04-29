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

                
                var carritoCmd = new SqlCommand(
                    "SELECT IdCarrito FROM Carritos WHERE IdUsuario = @IdUsuario",
                    conn);
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
                    conn);
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
                    "INSERT INTO Pedidos (IdUsuario, Fecha, Total, Estado) OUTPUT INSERTED.IdPedido VALUES (@IdUsuario, GETDATE(), @Total, 'Pendiente')",
                    conn);
                pedidoCmd.Parameters.AddWithValue("@IdUsuario", dto.IdUsuario);
                pedidoCmd.Parameters.AddWithValue("@Total",     total);
                int idPedido = (int)pedidoCmd.ExecuteScalar()!;

                
                foreach (var item in items)
                {
                    var detalleCmd = new SqlCommand(
                        @"INSERT INTO PedidoProductos (IdPedido, IdProducto, Cantidad, Precio)
                          VALUES (@IdPedido, @IdProducto, @Cantidad, @Precio)",
                        conn);
                    detalleCmd.Parameters.AddWithValue("@IdPedido",   idPedido);
                    detalleCmd.Parameters.AddWithValue("@IdProducto", item.idProducto);
                    detalleCmd.Parameters.AddWithValue("@Cantidad",   item.cantidad);
                    detalleCmd.Parameters.AddWithValue("@Precio",     item.precio);
                    detalleCmd.ExecuteNonQuery();

                    var stockCmd = new SqlCommand(
                        "UPDATE Productos SET Stock = Stock - @Cantidad WHERE IdProducto = @IdProducto",
                        conn);
                    stockCmd.Parameters.AddWithValue("@Cantidad",   item.cantidad);
                    stockCmd.Parameters.AddWithValue("@IdProducto", item.idProducto);
                    stockCmd.ExecuteNonQuery();
                }

                
                var clearCmd = new SqlCommand(
                    "DELETE FROM CarritoProductos WHERE IdCarrito = @IdCarrito",
                    conn);
                clearCmd.Parameters.AddWithValue("@IdCarrito", idCarrito);
                clearCmd.ExecuteNonQuery();

                return Ok(new { message = "Pedido confirmado correctamente.", idPedido, total });
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

                var cmd = new SqlCommand(@"
                    SELECT pp.IdDetalle, pp.IdProducto, pp.Cantidad, pp.Precio,
                           p.Nombre AS NombreProducto
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
                        idDetalle      = (int)reader["IdDetalle"],
                        idProducto     = (int)reader["IdProducto"],
                        nombreProducto = reader["NombreProducto"].ToString(),
                        cantidad       = (int)reader["Cantidad"],
                        precio         = Convert.ToDecimal(reader["Precio"])
                    });
                }

                return Ok(detalles);
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
