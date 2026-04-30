using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using EnerGym.Models;

namespace EnerGym.Controllers
{
    [ApiController]
    [Route("api/mensajes")]
    public class MensajesController : ControllerBase
    {
        private readonly Database _db;

        public MensajesController(Database db)
        {
            _db = db;
        }

        [HttpPost]
        public IActionResult CrearMensaje([FromBody] CrearMensajeDto dto)
        {
            try
            {
                using var conn = _db.GetConnection();
                conn.Open();

                var cmd = new SqlCommand(
                    "INSERT INTO MensajesSoporte (IdUsuario, Nombre, Email, Asunto, Mensaje, Fecha, Leido, Respondido) OUTPUT INSERTED.IdMensaje VALUES (@IdUsuario, @Nombre, @Email, @Asunto, @Mensaje, GETDATE(), 0, 0)",
                    conn);
                cmd.Parameters.AddWithValue("@IdUsuario", (object?)dto.IdUsuario ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Nombre", dto.Nombre);
                cmd.Parameters.AddWithValue("@Email", dto.Email);
                cmd.Parameters.AddWithValue("@Asunto", dto.Asunto);
                cmd.Parameters.AddWithValue("@Mensaje", dto.Mensaje);

                int idMensaje = (int)cmd.ExecuteScalar()!;
                return Ok(new { message = "Mensaje enviado correctamente", idMensaje });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult GetMensajes()
        {
            try
            {
                using var conn = _db.GetConnection();
                conn.Open();

                var cmd = new SqlCommand(
                    "SELECT IdMensaje, IdUsuario, Nombre, Email, Asunto, Mensaje, Fecha, Leido, Respondido FROM MensajesSoporte ORDER BY Fecha DESC",
                    conn);

                var mensajes = new List<object>();
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    mensajes.Add(new
                    {
                        idMensaje = (int)reader["IdMensaje"],
                        idUsuario = reader["IdUsuario"] == DBNull.Value ? (int?)null : (int)reader["IdUsuario"],
                        nombre = reader["Nombre"].ToString(),
                        email = reader["Email"].ToString(),
                        asunto = reader["Asunto"].ToString(),
                        mensaje = reader["Mensaje"].ToString(),
                        fecha = ((DateTime)reader["Fecha"]).ToString("yyyy-MM-dd HH:mm:ss"),
                        leido = (bool)reader["Leido"],
                        respondido = (bool)reader["Respondido"]
                    });
                }

                return Ok(mensajes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("usuario/{idUsuario:int}")]
        public IActionResult GetMensajesUsuario(int idUsuario)
        {
            try
            {
                using var conn = _db.GetConnection();
                conn.Open();

                var cmd = new SqlCommand(
                    "SELECT IdMensaje, Asunto, Mensaje, Fecha, Leido, Respondido FROM MensajesSoporte WHERE IdUsuario = @IdUsuario ORDER BY Fecha DESC",
                    conn);
                cmd.Parameters.AddWithValue("@IdUsuario", idUsuario);

                var mensajes = new List<object>();
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    mensajes.Add(new
                    {
                        idMensaje = (int)reader["IdMensaje"],
                        asunto = reader["Asunto"].ToString(),
                        mensaje = reader["Mensaje"].ToString(),
                        fecha = ((DateTime)reader["Fecha"]).ToString("yyyy-MM-dd HH:mm:ss"),
                        leido = (bool)reader["Leido"],
                        respondido = (bool)reader["Respondido"]
                    });
                }

                return Ok(mensajes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPut("{idMensaje:int}/leido")]
        public IActionResult MarcarLeido(int idMensaje)
        {
            try
            {
                using var conn = _db.GetConnection();
                conn.Open();

                var cmd = new SqlCommand(
                    "UPDATE MensajesSoporte SET Leido = 1 WHERE IdMensaje = @IdMensaje",
                    conn);
                cmd.Parameters.AddWithValue("@IdMensaje", idMensaje);
                int filas = cmd.ExecuteNonQuery();

                if (filas == 0) return NotFound(new { error = "Mensaje no encontrado" });
                return Ok(new { message = "Mensaje marcado como leído" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPut("{idMensaje:int}/respondido")]
        public IActionResult MarcarRespondido(int idMensaje)
        {
            try
            {
                using var conn = _db.GetConnection();
                conn.Open();

                var cmd = new SqlCommand(
                    "UPDATE MensajesSoporte SET Respondido = 1 WHERE IdMensaje = @IdMensaje",
                    conn);
                cmd.Parameters.AddWithValue("@IdMensaje", idMensaje);
                int filas = cmd.ExecuteNonQuery();

                if (filas == 0) return NotFound(new { error = "Mensaje no encontrado" });
                return Ok(new { message = "Mensaje marcado como respondido" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpDelete("{idMensaje:int}")]
        public IActionResult EliminarMensaje(int idMensaje)
        {
            try
            {
                using var conn = _db.GetConnection();
                conn.Open();

                var cmd = new SqlCommand(
                    "DELETE FROM MensajesSoporte WHERE IdMensaje = @IdMensaje",
                    conn);
                cmd.Parameters.AddWithValue("@IdMensaje", idMensaje);
                int filas = cmd.ExecuteNonQuery();

                if (filas == 0) return NotFound(new { error = "Mensaje no encontrado" });
                return Ok(new { message = "Mensaje eliminado" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("no-leidos/count")]
        public IActionResult GetCountNoLeidos()
        {
            try
            {
                using var conn = _db.GetConnection();
                conn.Open();

                var cmd = new SqlCommand(
                    "SELECT COUNT(*) FROM MensajesSoporte WHERE Leido = 0",
                    conn);
                int count = (int)cmd.ExecuteScalar()!;
                return Ok(new { count });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
