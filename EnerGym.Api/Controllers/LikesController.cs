using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using EnerGym.Models;

namespace EnerGym.Controllers
{
    [ApiController]
    [Route("api/likes")]
    public class LikesController : ControllerBase
    {
        private readonly Database _db;

        public LikesController(Database db)
        {
            _db = db;
        }

        
        [HttpPost("toggle")]
        public IActionResult ToggleLike([FromBody] LikeDto dto)
        {
            try
            {
                using var conn = _db.GetConnection();
                conn.Open();

                
                var checkCmd = new SqlCommand(
                    "SELECT IdLike FROM LikesProductos WHERE IdUsuario = @IdUsuario AND IdProducto = @IdProducto",
                    conn);
                checkCmd.Parameters.AddWithValue("@IdUsuario",  dto.IdUsuario);
                checkCmd.Parameters.AddWithValue("@IdProducto", dto.IdProducto);
                var result = checkCmd.ExecuteScalar();

                if (result != null && result != DBNull.Value)
                {
                    
                    var delCmd = new SqlCommand(
                        "DELETE FROM LikesProductos WHERE IdUsuario = @IdUsuario AND IdProducto = @IdProducto",
                        conn);
                    delCmd.Parameters.AddWithValue("@IdUsuario",  dto.IdUsuario);
                    delCmd.Parameters.AddWithValue("@IdProducto", dto.IdProducto);
                    delCmd.ExecuteNonQuery();
                    return Ok(new { liked = false, message = "Like eliminado." });
                }
                else
                {
                    
                    var insertCmd = new SqlCommand(
                        "INSERT INTO LikesProductos (IdUsuario, IdProducto) VALUES (@IdUsuario, @IdProducto)",
                        conn);
                    insertCmd.Parameters.AddWithValue("@IdUsuario",  dto.IdUsuario);
                    insertCmd.Parameters.AddWithValue("@IdProducto", dto.IdProducto);
                    insertCmd.ExecuteNonQuery();
                    return Ok(new { liked = true, message = "Like añadido." });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        
        [HttpGet("producto/{idProducto:int}/count")]
        public IActionResult GetLikesProducto(int idProducto)
        {
            try
            {
                using var conn = _db.GetConnection();
                conn.Open();

                var cmd = new SqlCommand(
                    "SELECT COUNT(*) FROM LikesProductos WHERE IdProducto = @IdProducto",
                    conn);
                cmd.Parameters.AddWithValue("@IdProducto", idProducto);
                int totalLikes = (int)cmd.ExecuteScalar()!;

                return Ok(new { idProducto, totalLikes });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        
        [HttpGet("{idUsuario:int}")]
        public IActionResult GetLikesUsuario(int idUsuario)
        {
            try
            {
                using var conn = _db.GetConnection();
                conn.Open();

                var cmd = new SqlCommand(@"
                    SELECT p.IdProducto, p.Nombre, p.Precio, p.Imagen, p.Stock
                    FROM LikesProductos l
                    INNER JOIN Productos p ON l.IdProducto = p.IdProducto
                    WHERE l.IdUsuario = @IdUsuario
                    ORDER BY l.IdLike DESC",
                    conn);
                cmd.Parameters.AddWithValue("@IdUsuario", idUsuario);

                var likes = new List<object>();
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    likes.Add(new
                    {
                        idProducto = (int)reader["IdProducto"],
                        nombre     = reader["Nombre"].ToString(),
                        precio     = Convert.ToDecimal(reader["Precio"]),
                        stock      = (int)reader["Stock"],
                        imagen     = reader["Imagen"] == DBNull.Value ? "" : reader["Imagen"].ToString()
                    });
                }

                return Ok(likes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
