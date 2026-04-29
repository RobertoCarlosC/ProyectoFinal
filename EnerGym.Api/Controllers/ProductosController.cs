using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using EnerGym.Models;

namespace EnerGym.Controllers
{
    [ApiController]
    [Route("api/productos")]
    public class ProductosController : ControllerBase
    {
        private readonly Database _db;

        public ProductosController(Database db)
        {
            _db = db;
        }

        
        [HttpGet]
        public IActionResult GetProductos([FromQuery] int? idUsuario)
        {
            try
            {
                using var conn = _db.GetConnection();
                conn.Open();

                
                string sql = @"
                    SELECT
                        p.IdProducto,
                        p.Nombre,
                        p.Descripcion,
                        p.Precio,
                        p.Stock,
                        p.Imagen,
                        p.IdCategoria,
                        c.Nombre AS NombreCategoria,
                        CASE WHEN EXISTS(SELECT 1 FROM LikesProductos WHERE IdProducto = p.IdProducto AND IdUsuario = @IdUsuario) THEN 1 ELSE 0 END AS TieneLike,
                        (SELECT COUNT(*) FROM LikesProductos WHERE IdProducto = p.IdProducto) AS TotalLikes
                    FROM Productos p
                    LEFT JOIN Categorias c ON p.IdCategoria = c.IdCategoria
                    ORDER BY p.IdProducto DESC";

                var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@IdUsuario", (object?)idUsuario ?? DBNull.Value);

                var productos = new List<object>();
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    
                    var imagen = reader["Imagen"] == DBNull.Value ? "" : reader["Imagen"].ToString()!;

                    productos.Add(new
                    {
                        idProducto      = (int)reader["IdProducto"],
                        nombre          = reader["Nombre"].ToString(),
                        descripcion     = reader["Descripcion"] == DBNull.Value ? "" : reader["Descripcion"].ToString(),
                        precio          = Convert.ToDecimal(reader["Precio"]),
                        stock           = (int)reader["Stock"],
                        imagen          = imagen,
                        idCategoria     = (int)reader["IdCategoria"],
                        nombreCategoria = reader["NombreCategoria"] == DBNull.Value ? "" : reader["NombreCategoria"].ToString(),
                        tieneLike       = (int)reader["TieneLike"] == 1,
                        totalLikes      = (int)reader["TotalLikes"]
                    });
                }

                return Ok(productos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        
        [HttpGet("categorias")]
        public IActionResult GetCategorias()
        {
            try
            {
                using var conn = _db.GetConnection();
                conn.Open();

                var cmd = new SqlCommand("SELECT IdCategoria, Nombre, Descripcion FROM Categorias ORDER BY Nombre", conn);
                var categorias = new List<object>();
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    categorias.Add(new
                    {
                        idCategoria = (int)reader["IdCategoria"],
                        nombre      = reader["Nombre"].ToString(),
                        descripcion = reader["Descripcion"] == DBNull.Value ? "" : reader["Descripcion"].ToString()
                    });
                }

                return Ok(categorias);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        
        [HttpGet("{id:int}")]
        public IActionResult GetProducto(int id, [FromQuery] int? idUsuario)
        {
            try
            {
                using var conn = _db.GetConnection();
                conn.Open();

                var cmd = new SqlCommand(
                    @"SELECT p.IdProducto, p.Nombre, p.Descripcion, p.Precio, p.Stock, p.Imagen,
                             p.IdCategoria, c.Nombre AS NombreCategoria,
                             CASE WHEN EXISTS(SELECT 1 FROM LikesProductos WHERE IdProducto = p.IdProducto AND IdUsuario = @IdUsuario) THEN 1 ELSE 0 END AS TieneLike,
                             (SELECT COUNT(*) FROM LikesProductos WHERE IdProducto = p.IdProducto) AS TotalLikes
                      FROM Productos p
                      LEFT JOIN Categorias c ON p.IdCategoria = c.IdCategoria
                      WHERE p.IdProducto = @Id",
                    conn);
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.Parameters.AddWithValue("@IdUsuario", (object?)idUsuario ?? DBNull.Value);

                using var reader = cmd.ExecuteReader();
                if (!reader.Read())
                    return NotFound(new { error = "Producto no encontrado." });

                return Ok(new
                {
                    idProducto      = (int)reader["IdProducto"],
                    nombre          = reader["Nombre"].ToString(),
                    descripcion     = reader["Descripcion"] == DBNull.Value ? "" : reader["Descripcion"].ToString(),
                    precio          = Convert.ToDecimal(reader["Precio"]),
                    stock           = (int)reader["Stock"],
                    imagen          = reader["Imagen"] == DBNull.Value ? "" : reader["Imagen"].ToString(),
                    idCategoria     = (int)reader["IdCategoria"],
                    nombreCategoria = reader["NombreCategoria"] == DBNull.Value ? "" : reader["NombreCategoria"].ToString(),
                    tieneLike       = (int)reader["TieneLike"] == 1,
                    totalLikes      = (int)reader["TotalLikes"]
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        
        [HttpPost]
        public IActionResult CreateProducto([FromBody] ProductoDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Nombre))
                return BadRequest(new { error = "El nombre es obligatorio." });

            try
            {
                using var conn = _db.GetConnection();
                conn.Open();

                var cmd = new SqlCommand(
                    @"INSERT INTO Productos (Nombre, Descripcion, Precio, Stock, Imagen, IdCategoria)
                      VALUES (@Nombre, @Descripcion, @Precio, @Stock, @Imagen, @IdCategoria)",
                    conn);
                cmd.Parameters.AddWithValue("@Nombre",      dto.Nombre);
                cmd.Parameters.AddWithValue("@Descripcion", dto.Descripcion ?? "");
                cmd.Parameters.AddWithValue("@Precio",      dto.Precio);
                cmd.Parameters.AddWithValue("@Stock",       dto.Stock);
                cmd.Parameters.AddWithValue("@Imagen",      dto.Imagen ?? "");
                cmd.Parameters.AddWithValue("@IdCategoria", dto.IdCategoria);
                cmd.ExecuteNonQuery();

                return Ok(new { message = "Producto creado correctamente." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        
        [HttpPut("{id:int}")]
        public IActionResult UpdateProducto(int id, [FromBody] ProductoDto dto)
        {
            try
            {
                using var conn = _db.GetConnection();
                conn.Open();

                var cmd = new SqlCommand(
                    @"UPDATE Productos
                      SET Nombre       = @Nombre,
                          Descripcion  = @Descripcion,
                          Precio       = @Precio,
                          Stock        = @Stock,
                          Imagen       = @Imagen,
                          IdCategoria  = @IdCategoria
                      WHERE IdProducto = @Id",
                    conn);
                cmd.Parameters.AddWithValue("@Id",          id);
                cmd.Parameters.AddWithValue("@Nombre",      dto.Nombre);
                cmd.Parameters.AddWithValue("@Descripcion", dto.Descripcion ?? "");
                cmd.Parameters.AddWithValue("@Precio",      dto.Precio);
                cmd.Parameters.AddWithValue("@Stock",       dto.Stock);
                cmd.Parameters.AddWithValue("@Imagen",      dto.Imagen ?? "");
                cmd.Parameters.AddWithValue("@IdCategoria", dto.IdCategoria);
                int rows = cmd.ExecuteNonQuery();

                if (rows == 0) return NotFound(new { error = "Producto no encontrado." });
                return Ok(new { message = "Producto actualizado." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        
        [HttpDelete("{id:int}")]
        public IActionResult DeleteProducto(int id)
        {
            try
            {
                using var conn = _db.GetConnection();
                conn.Open();

                
                var delLikes = new SqlCommand("DELETE FROM LikesProductos WHERE IdProducto = @Id", conn);
                delLikes.Parameters.AddWithValue("@Id", id);
                delLikes.ExecuteNonQuery();

                var delCarrito = new SqlCommand("DELETE FROM CarritoProductos WHERE IdProducto = @Id", conn);
                delCarrito.Parameters.AddWithValue("@Id", id);
                delCarrito.ExecuteNonQuery();

                var cmd = new SqlCommand("DELETE FROM Productos WHERE IdProducto = @Id", conn);
                cmd.Parameters.AddWithValue("@Id", id);
                int rows = cmd.ExecuteNonQuery();

                if (rows == 0) return NotFound(new { error = "Producto no encontrado." });
                return Ok(new { message = "Producto eliminado." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
