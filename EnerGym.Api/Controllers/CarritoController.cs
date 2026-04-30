using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using EnerGym.Models;

namespace EnerGym.Controllers
{
    [ApiController]
    [Route("api/carrito")]
    public class CarritoController : ControllerBase
    {
        private readonly Database _db;

        public CarritoController(Database db)
        {
            _db = db;
        }

        
        private int ObtenerOCrearCarrito(SqlConnection conn, int idUsuario)
        {
            var getCmd = new SqlCommand(
                "SELECT IdCarrito FROM Carritos WHERE IdUsuario = @IdUsuario",
                conn);
            getCmd.Parameters.AddWithValue("@IdUsuario", idUsuario);
            var result = getCmd.ExecuteScalar();

            if (result != null && result != DBNull.Value)
                return (int)result;

            
            var insertCmd = new SqlCommand(
                "INSERT INTO Carritos (IdUsuario, FechaCreacion) OUTPUT INSERTED.IdCarrito VALUES (@IdUsuario, GETDATE())",
                conn);
            insertCmd.Parameters.AddWithValue("@IdUsuario", idUsuario);
            return (int)insertCmd.ExecuteScalar()!;
        }

        
        [HttpGet("{idUsuario:int}")]
        public IActionResult GetCarrito(int idUsuario)
        {
            try
            {
                using var conn = _db.GetConnection();
                conn.Open();

                int idCarrito = ObtenerOCrearCarrito(conn, idUsuario);

                var cmd = new SqlCommand(@"
                    SELECT
                        cp.Id,
                        cp.IdCarrito,
                        cp.IdProducto,
                        cp.Cantidad,
                        p.Nombre  AS NombreProducto,
                        p.Imagen,
                        p.Precio
                    FROM CarritoProductos cp
                    INNER JOIN Productos p ON cp.IdProducto = p.IdProducto
                    WHERE cp.IdCarrito = @IdCarrito", conn);
                cmd.Parameters.AddWithValue("@IdCarrito", idCarrito);

                var items = new List<object>();
                decimal total = 0;
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    decimal precio   = Convert.ToDecimal(reader["Precio"]);
                    int cantidad     = (int)reader["Cantidad"];
                    decimal subtotal = precio * cantidad;
                    total += subtotal;

                    items.Add(new
                    {
                        id             = (int)reader["Id"],
                        idCarrito      = (int)reader["IdCarrito"],
                        idProducto     = (int)reader["IdProducto"],
                        nombreProducto = reader["NombreProducto"].ToString(),
                        imagen         = reader["Imagen"] == DBNull.Value ? "" : reader["Imagen"].ToString(),
                        precio,
                        cantidad,
                        subtotal
                    });
                }

                return Ok(new { idCarrito, items, total });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        
        [HttpPost("agregar")]
        public IActionResult AgregarProducto([FromBody] AddCarritoDto dto)
        {
            if (dto.Cantidad <= 0)
                return BadRequest(new { error = "La cantidad debe ser mayor que 0." });

            try
            {
                using var conn = _db.GetConnection();
                conn.Open();

                var prodCmd = new SqlCommand(
                    "SELECT Stock FROM Productos WHERE IdProducto = @IdProducto",
                    conn);
                prodCmd.Parameters.AddWithValue("@IdProducto", dto.IdProducto);
                var prodResult = prodCmd.ExecuteScalar();
                if (prodResult == null || prodResult == DBNull.Value)
                    return BadRequest(new { error = "El producto no existe." });

                int stock = (int)prodResult;

                int idCarrito = ObtenerOCrearCarrito(conn, dto.IdUsuario);

                var checkCmd = new SqlCommand(
                    "SELECT Id, Cantidad FROM CarritoProductos WHERE IdCarrito = @IdCarrito AND IdProducto = @IdProducto",
                    conn);
                checkCmd.Parameters.AddWithValue("@IdCarrito",  idCarrito);
                checkCmd.Parameters.AddWithValue("@IdProducto", dto.IdProducto);

                using var reader = checkCmd.ExecuteReader();
                if (reader.Read())
                {
                    int itemId          = (int)reader["Id"];
                    int cantidadActual  = (int)reader["Cantidad"];
                    reader.Close();

                    if (cantidadActual + dto.Cantidad > stock)
                        return BadRequest(new { error = $"Stock insuficiente. Disponible: {stock}, en carrito: {cantidadActual}, solicitado: {dto.Cantidad}." });

                    var updateCmd = new SqlCommand(
                        "UPDATE CarritoProductos SET Cantidad = @Cantidad WHERE Id = @Id",
                        conn);
                    updateCmd.Parameters.AddWithValue("@Cantidad", cantidadActual + dto.Cantidad);
                    updateCmd.Parameters.AddWithValue("@Id",       itemId);
                    updateCmd.ExecuteNonQuery();
                }
                else
                {
                    reader.Close();
                    if (dto.Cantidad > stock)
                        return BadRequest(new { error = $"Stock insuficiente. Disponible: {stock}, solicitado: {dto.Cantidad}." });

                    var insertCmd = new SqlCommand(
                        "INSERT INTO CarritoProductos (IdCarrito, IdProducto, Cantidad) VALUES (@IdCarrito, @IdProducto, @Cantidad)",
                        conn);
                    insertCmd.Parameters.AddWithValue("@IdCarrito",  idCarrito);
                    insertCmd.Parameters.AddWithValue("@IdProducto", dto.IdProducto);
                    insertCmd.Parameters.AddWithValue("@Cantidad",   dto.Cantidad);
                    insertCmd.ExecuteNonQuery();
                }

                return Ok(new { message = "Producto añadido al carrito." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        
        [HttpPut("item/{id:int}")]
        public IActionResult UpdateCantidad(int id, [FromBody] UpdateCantidadDto dto)
        {
            try
            {
                using var conn = _db.GetConnection();
                conn.Open();

                if (dto.Cantidad <= 0)
                {
                    var delCmd = new SqlCommand("DELETE FROM CarritoProductos WHERE Id = @Id", conn);
                    delCmd.Parameters.AddWithValue("@Id", id);
                    int deleted = delCmd.ExecuteNonQuery();
                    if (deleted == 0) return NotFound(new { error = "Item no encontrado." });
                    return Ok(new { message = "Producto eliminado del carrito." });
                }

                var stockCmd = new SqlCommand(
                    @"SELECT cp.IdProducto, p.Stock FROM CarritoProductos cp
                      INNER JOIN Productos p ON cp.IdProducto = p.IdProducto
                      WHERE cp.Id = @Id", conn);
                stockCmd.Parameters.AddWithValue("@Id", id);
                using var r = stockCmd.ExecuteReader();
                if (!r.Read()) return NotFound(new { error = "Item no encontrado." });
                int stock = (int)r["Stock"];
                r.Close();

                if (dto.Cantidad > stock)
                    return BadRequest(new { error = $"Stock insuficiente. Disponible: {stock}." });

                var cmd = new SqlCommand(
                    "UPDATE CarritoProductos SET Cantidad = @Cantidad WHERE Id = @Id",
                    conn);
                cmd.Parameters.AddWithValue("@Cantidad", dto.Cantidad);
                cmd.Parameters.AddWithValue("@Id",       id);
                cmd.ExecuteNonQuery();

                return Ok(new { message = "Cantidad actualizada." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        
        [HttpDelete("item/{id:int}")]
        public IActionResult EliminarItem(int id)
        {
            try
            {
                using var conn = _db.GetConnection();
                conn.Open();

                var cmd = new SqlCommand("DELETE FROM CarritoProductos WHERE Id = @Id", conn);
                cmd.Parameters.AddWithValue("@Id", id);
                int rows = cmd.ExecuteNonQuery();

                if (rows == 0) return NotFound(new { error = "Item no encontrado." });
                return Ok(new { message = "Producto eliminado del carrito." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
