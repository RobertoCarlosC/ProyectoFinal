using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using EnerGym.Models;

namespace EnerGym.Controllers
{
    [ApiController]
    [Route("api/usuarios")]
    public class UsuariosController : ControllerBase
    {
        private readonly Database _db;

        public UsuariosController(Database db)
        {
            _db = db;
        }

        
        [HttpGet("{idUsuario:int}/perfil")]
        public IActionResult GetPerfil(int idUsuario)
        {
            try
            {
                using var conn = _db.GetConnection();
                conn.Open();

                var cmd = new SqlCommand(
                    @"SELECT IdUsuario, Nombre, Email, Telefono, Direccion, Ciudad, CodigoPostal, FotoPerfil, FechaRegistro
                      FROM Usuarios
                      WHERE IdUsuario = @IdUsuario",
                    conn);
                cmd.Parameters.AddWithValue("@IdUsuario", idUsuario);

                using var reader = cmd.ExecuteReader();
                if (!reader.Read())
                    return NotFound(new { error = "Usuario no encontrado." });

                var perfil = new
                {
                    idUsuario = (int)reader["IdUsuario"],
                    nombre = reader["Nombre"].ToString(),
                    email = reader["Email"].ToString(),
                    telefono = reader["Telefono"] == DBNull.Value ? "" : reader["Telefono"].ToString(),
                    direccion = reader["Direccion"] == DBNull.Value ? "" : reader["Direccion"].ToString(),
                    ciudad = reader["Ciudad"] == DBNull.Value ? "" : reader["Ciudad"].ToString(),
                    codigoPostal = reader["CodigoPostal"] == DBNull.Value ? "" : reader["CodigoPostal"].ToString(),
                    fotoPerfil = reader["FotoPerfil"] == DBNull.Value ? "" : reader["FotoPerfil"].ToString(),
                    fechaRegistro = reader["FechaRegistro"]
                };

                return Ok(perfil);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        
        [HttpPut("{idUsuario:int}/editar-perfil")]
        public IActionResult EditarPerfil(int idUsuario, [FromBody] EditarPerfilDto dto)
        {
            if (idUsuario != dto.IdUsuario)
                return BadRequest(new { error = "ID de usuario no válido." });

            if (string.IsNullOrWhiteSpace(dto.Nombre) || string.IsNullOrWhiteSpace(dto.Email))
                return BadRequest(new { error = "Nombre y Email son obligatorios." });

            try
            {
                using var conn = _db.GetConnection();
                conn.Open();

                
                var checkCmd = new SqlCommand(
                    "SELECT COUNT(*) FROM Usuarios WHERE Email = @Email AND IdUsuario != @IdUsuario",
                    conn);
                checkCmd.Parameters.AddWithValue("@Email", dto.Email.Trim().ToLower());
                checkCmd.Parameters.AddWithValue("@IdUsuario", idUsuario);
                int existe = (int)checkCmd.ExecuteScalar()!;
                if (existe > 0)
                    return Conflict(new { error = "El email ya está registrado a otro usuario." });

                var updateCmd = new SqlCommand(@"
                    UPDATE Usuarios
                    SET Nombre = @Nombre,
                        Email = @Email,
                        Telefono = @Telefono,
                        Direccion = @Direccion,
                        Ciudad = @Ciudad,
                        CodigoPostal = @CodigoPostal,
                        FotoPerfil = @FotoPerfil
                    WHERE IdUsuario = @IdUsuario",
                    conn);

                updateCmd.Parameters.AddWithValue("@Nombre", dto.Nombre.Trim());
                updateCmd.Parameters.AddWithValue("@Email", dto.Email.Trim().ToLower());
                updateCmd.Parameters.AddWithValue("@Telefono", (object?)dto.Telefono ?? DBNull.Value);
                updateCmd.Parameters.AddWithValue("@Direccion", (object?)dto.Direccion ?? DBNull.Value);
                updateCmd.Parameters.AddWithValue("@Ciudad", (object?)dto.Ciudad ?? DBNull.Value);
                updateCmd.Parameters.AddWithValue("@CodigoPostal", (object?)dto.CodigoPostal ?? DBNull.Value);
                updateCmd.Parameters.AddWithValue("@FotoPerfil", (object?)dto.FotoPerfil ?? DBNull.Value);
                updateCmd.Parameters.AddWithValue("@IdUsuario", idUsuario);

                int filas = updateCmd.ExecuteNonQuery();
                if (filas == 0)
                    return NotFound(new { error = "Usuario no encontrado." });

                return Ok(new { message = "Perfil actualizado correctamente." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        
        [HttpPut("{idUsuario:int}/cambiar-contraseña")]
        public IActionResult CambiarContraseña(int idUsuario, [FromBody] CambiarContraseñaDto dto)
        {
            if (idUsuario != dto.IdUsuario)
                return BadRequest(new { error = "ID de usuario no válido." });

            if (string.IsNullOrWhiteSpace(dto.ContraseñaActual) || 
                string.IsNullOrWhiteSpace(dto.ContraseñaNueva) ||
                string.IsNullOrWhiteSpace(dto.ConfirmarContraseña))
                return BadRequest(new { error = "Todos los campos son obligatorios." });

            if (dto.ContraseñaNueva != dto.ConfirmarContraseña)
                return BadRequest(new { error = "Las contraseñas no coinciden." });

            if (dto.ContraseñaNueva.Length < 6)
                return BadRequest(new { error = "La contraseña debe tener al menos 6 caracteres." });

            try
            {
                using var conn = _db.GetConnection();
                conn.Open();

                
                var cmd = new SqlCommand(
                    "SELECT PasswordHash FROM Usuarios WHERE IdUsuario = @IdUsuario",
                    conn);
                cmd.Parameters.AddWithValue("@IdUsuario", idUsuario);
                var hashActual = cmd.ExecuteScalar();

                if (hashActual == null || hashActual == DBNull.Value)
                    return NotFound(new { error = "Usuario no encontrado." });

                
                string storedHash = hashActual.ToString()!;
                bool passwordOk = BCrypt.Net.BCrypt.Verify(dto.ContraseñaActual, storedHash);
                if (!passwordOk)
                    return Unauthorized(new { error = "Contraseña actual incorrecta." });

                
                string nuevoHash = BCrypt.Net.BCrypt.HashPassword(dto.ContraseñaNueva);

                
                var updateCmd = new SqlCommand(
                    "UPDATE Usuarios SET PasswordHash = @PasswordHash WHERE IdUsuario = @IdUsuario",
                    conn);
                updateCmd.Parameters.AddWithValue("@PasswordHash", nuevoHash);
                updateCmd.Parameters.AddWithValue("@IdUsuario", idUsuario);
                updateCmd.ExecuteNonQuery();

                return Ok(new { message = "Contraseña cambiada correctamente." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        
        [HttpGet("{idUsuario:int}/pedidos")]
        public IActionResult GetPedidosUsuario(int idUsuario)
        {
            try
            {
                using var conn = _db.GetConnection();
                conn.Open();

                var cmd = new SqlCommand(@"
                    SELECT IdPedido, Fecha, Total, Estado
                    FROM Pedidos
                    WHERE IdUsuario = @IdUsuario
                    ORDER BY Fecha DESC",
                    conn);
                cmd.Parameters.AddWithValue("@IdUsuario", idUsuario);

                var pedidos = new List<object>();
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    pedidos.Add(new
                    {
                        idPedido = (int)reader["IdPedido"],
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

        
        [HttpGet("{idUsuario:int}/pedidos/{idPedido:int}")]
        public IActionResult GetDetallePedido(int idUsuario, int idPedido)
        {
            try
            {
                using var conn = _db.GetConnection();
                conn.Open();

                
                var checkCmd = new SqlCommand(
                    "SELECT IdPedido FROM Pedidos WHERE IdPedido = @IdPedido AND IdUsuario = @IdUsuario",
                    conn);
                checkCmd.Parameters.AddWithValue("@IdPedido", idPedido);
                checkCmd.Parameters.AddWithValue("@IdUsuario", idUsuario);
                var result = checkCmd.ExecuteScalar();

                if (result == null || result == DBNull.Value)
                    return NotFound(new { error = "Pedido no encontrado." });

                
                var pedidoCmd = new SqlCommand(@"
                    SELECT IdPedido, Fecha, Total, Estado
                    FROM Pedidos
                    WHERE IdPedido = @IdPedido",
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
                            fecha = reader["Fecha"],
                            total = Convert.ToDecimal(reader["Total"]),
                            estado = reader["Estado"].ToString()
                        };
                    }
                }

                
                var productosCmd = new SqlCommand(@"
                    SELECT pp.IdDetalle, pp.IdProducto, pp.Cantidad, pp.Precio, p.Nombre, p.Imagen
                    FROM PedidoProductos pp
                    INNER JOIN Productos p ON pp.IdProducto = p.IdProducto
                    WHERE pp.IdPedido = @IdPedido",
                    conn);
                productosCmd.Parameters.AddWithValue("@IdPedido", idPedido);

                var detalles = new List<object>();
                using (var reader = productosCmd.ExecuteReader())
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
    }
}
