using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using EnerGym.Models;

namespace EnerGym.Controllers
{
    [ApiController]
    [Route("api")]
    public class AuthController : ControllerBase
    {
        private readonly Database _db;

        public AuthController(Database db)
        {
            _db = db;
        }

        
        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Nombre) ||
                string.IsNullOrWhiteSpace(dto.Email) ||
                string.IsNullOrWhiteSpace(dto.Password))
            {
                return BadRequest(new { error = "Todos los campos son obligatorios." });
            }

            try
            {
                using var conn = _db.GetConnection();
                conn.Open();

                
                var checkCmd = new SqlCommand(
                    "SELECT COUNT(*) FROM Usuarios WHERE Email = @Email",
                    conn);
                checkCmd.Parameters.AddWithValue("@Email", dto.Email.Trim().ToLower());
                int existe = (int)checkCmd.ExecuteScalar()!;
                if (existe > 0)
                    return Conflict(new { error = "El email ya está registrado." });

                
                string hash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

                
                var cmd = new SqlCommand(
                    @"INSERT INTO Usuarios (Nombre, Email, PasswordHash, IdRol, FechaRegistro)
                      VALUES (@Nombre, @Email, @PasswordHash, 2, GETDATE())",
                    conn);
                cmd.Parameters.AddWithValue("@Nombre", dto.Nombre.Trim());
                cmd.Parameters.AddWithValue("@Email", dto.Email.Trim().ToLower());
                cmd.Parameters.AddWithValue("@PasswordHash", hash);
                cmd.ExecuteNonQuery();

                return Ok(new { message = "Usuario registrado correctamente." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
                return BadRequest(new { error = "Email y contraseña son obligatorios." });

            try
            {
                using var conn = _db.GetConnection();
                conn.Open();

                
                var cmd = new SqlCommand(
                    @"SELECT IdUsuario, Nombre, Email, PasswordHash, IdRol
                      FROM Usuarios
                      WHERE Email = @Email",
                    conn);
                cmd.Parameters.AddWithValue("@Email", dto.Email.Trim().ToLower());

                using var reader = cmd.ExecuteReader();
                if (!reader.Read())
                    return Unauthorized(new { error = "Email o contraseña incorrectos." });

                string storedHash = reader["PasswordHash"].ToString()!;

                
                bool passwordOk = BCrypt.Net.BCrypt.Verify(dto.Password, storedHash);
                if (!passwordOk)
                    return Unauthorized(new { error = "Email o contraseña incorrectos." });

                int idUsuario = (int)reader["IdUsuario"];
                string nombre   = reader["Nombre"].ToString()!;
                string email    = reader["Email"].ToString()!;
                int idRol       = (int)reader["IdRol"];

                
                return Ok(new
                {
                    idUsuario,
                    nombre,
                    email,
                    idRol   
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        
        [HttpGet("usuarios")]
        public IActionResult GetUsuarios()
        {
            try
            {
                using var conn = _db.GetConnection();
                conn.Open();

                var cmd = new SqlCommand(
                    @"SELECT u.IdUsuario, u.Nombre, u.Email, u.FechaRegistro, r.Nombre AS NombreRol
                      FROM Usuarios u
                      INNER JOIN Roles r ON u.IdRol = r.IdRol
                      ORDER BY u.FechaRegistro DESC",
                    conn);

                var usuarios = new List<object>();
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    usuarios.Add(new
                    {
                        idUsuario     = (int)reader["IdUsuario"],
                        nombre        = reader["Nombre"].ToString(),
                        email         = reader["Email"].ToString(),
                        fechaRegistro = reader["FechaRegistro"],
                        rol           = reader["NombreRol"].ToString()
                    });
                }

                return Ok(usuarios);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
