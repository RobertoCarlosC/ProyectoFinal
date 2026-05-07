using Microsoft.Data.SqlClient;
using EnerGym.Models;
using EnerGym.Repositories.Interfaces;
using EnerGym.Infrastructure;

namespace EnerGym.Repositories.Implementations
{
    public class UsuarioRepository : BaseRepository, IUsuarioRepository
    {
        public UsuarioRepository(Database db) : base(db) { }

        public async Task<Usuario?> GetByIdAsync(int id)
        {
            return await WithConnectionAsync(async conn =>
            {
                using var cmd = new SqlCommand(
                    "SELECT IdUsuario, Nombre, Email, IdRol, FechaRegistro FROM Usuarios WHERE IdUsuario = @Id", conn);
                cmd.Parameters.Add(SqlParameterHelper.Int("@Id", id));

                using var reader = await cmd.ExecuteReaderAsync();
                if (!await reader.ReadAsync()) return null;

                return new Usuario
                {
                    IdUsuario = (int)reader["IdUsuario"],
                    Nombre = reader["Nombre"].ToString()!,
                    Email = reader["Email"].ToString()!,
                    IdRol = (int)reader["IdRol"],
                    FechaRegistro = (DateTime)reader["FechaRegistro"]
                };
            });
        }

        public async Task<PerfilDto?> GetPerfilAsync(int id)
        {
            return await WithConnectionAsync(async conn =>
            {
                using var cmd = new SqlCommand(
                    @"SELECT IdUsuario, Nombre, Email, Telefono, Direccion, Ciudad, CodigoPostal, FotoPerfil, FechaRegistro
                      FROM Usuarios WHERE IdUsuario = @Id", conn);
                cmd.Parameters.Add(SqlParameterHelper.Int("@Id", id));

                using var reader = await cmd.ExecuteReaderAsync();
                if (!await reader.ReadAsync()) return null;

                return MapPerfil(reader);
            });
        }

        public async Task<bool> UpdatePerfilAsync(EditarPerfilDto dto)
        {
            return await WithConnectionAsync(async conn =>
            {
                using var cmd = new SqlCommand(@"
                    UPDATE Usuarios
                    SET Nombre = @Nombre, Email = @Email, Telefono = @Telefono,
                        Direccion = @Direccion, Ciudad = @Ciudad, CodigoPostal = @CodigoPostal, FotoPerfil = @FotoPerfil
                    WHERE IdUsuario = @Id", conn);

                cmd.Parameters.Add(SqlParameterHelper.String("@Nombre", 100, dto.Nombre));
                cmd.Parameters.Add(SqlParameterHelper.String("@Email", 150, dto.Email));
                cmd.Parameters.Add(SqlParameterHelper.String("@Telefono", 20, dto.Telefono));
                cmd.Parameters.Add(SqlParameterHelper.String("@Direccion", 255, dto.Direccion));
                cmd.Parameters.Add(SqlParameterHelper.String("@Ciudad", 100, dto.Ciudad));
                cmd.Parameters.Add(SqlParameterHelper.String("@CodigoPostal", 10, dto.CodigoPostal));
                cmd.Parameters.Add(SqlParameterHelper.String("@FotoPerfil", 500, dto.FotoPerfil));
                cmd.Parameters.Add(SqlParameterHelper.Int("@Id", dto.IdUsuario));

                return await cmd.ExecuteNonQueryAsync() > 0;
            });
        }

        public async Task<bool> UpdatePasswordAsync(int idUsuario, string newHash)
        {
            return await WithConnectionAsync(async conn =>
            {
                using var cmd = new SqlCommand(
                    "UPDATE Usuarios SET PasswordHash = @PasswordHash WHERE IdUsuario = @Id", conn);
                cmd.Parameters.Add(SqlParameterHelper.String("@PasswordHash", 255, newHash));
                cmd.Parameters.Add(SqlParameterHelper.Int("@Id", idUsuario));
                return await cmd.ExecuteNonQueryAsync() > 0;
            });
        }

        public async Task<string?> GetPasswordHashAsync(int idUsuario)
        {
            return await WithConnectionAsync(async conn =>
            {
                using var cmd = new SqlCommand(
                    "SELECT PasswordHash FROM Usuarios WHERE IdUsuario = @Id", conn);
                cmd.Parameters.Add(SqlParameterHelper.Int("@Id", idUsuario));
                var result = await cmd.ExecuteScalarAsync();
                return result == null || result == DBNull.Value ? null : result.ToString();
            });
        }

        public async Task<bool> EmailExistsAsync(string email, int? excludeId = null)
        {
            return await WithConnectionAsync(async conn =>
            {
                var sql = "SELECT COUNT(*) FROM Usuarios WHERE Email = @Email";
                if (excludeId.HasValue) sql += " AND IdUsuario != @ExcludeId";

                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add(SqlParameterHelper.String("@Email", 150, email.ToLower()));
                if (excludeId.HasValue)
                    cmd.Parameters.Add(SqlParameterHelper.Int("@ExcludeId", excludeId.Value));

                var result = await cmd.ExecuteScalarAsync();
                return result != null && (int)result > 0;
            });
        }

        public async Task<List<UsuarioListDto>> GetAllAsync()
        {
            return await WithConnectionAsync(async conn =>
            {
                var list = new List<UsuarioListDto>();
                using var cmd = new SqlCommand(
                    "SELECT IdUsuario, Nombre, Email, FechaRegistro, IdRol FROM Usuarios ORDER BY FechaRegistro DESC", conn);
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    list.Add(new UsuarioListDto
                    {
                        IdUsuario = (int)reader["IdUsuario"],
                        Nombre = reader["Nombre"].ToString()!,
                        Email = reader["Email"].ToString()!,
                        FechaRegistro = (DateTime)reader["FechaRegistro"],
                        IdRol = (int)reader["IdRol"],
                        Rol = (int)reader["IdRol"] == 1 ? "admin" : "usuario"
                    });
                }
                return list;
            });
        }

        public async Task<bool> UpdateAsync(EditarUsuarioAdminDto dto)
        {
            return await WithConnectionAsync(async conn =>
            {
                using var cmd = new SqlCommand(
                    "UPDATE Usuarios SET Nombre = @Nombre, Email = @Email, IdRol = @IdRol WHERE IdUsuario = @Id", conn);
                cmd.Parameters.Add(SqlParameterHelper.String("@Nombre", 100, dto.Nombre));
                cmd.Parameters.Add(SqlParameterHelper.String("@Email", 150, dto.Email));
                cmd.Parameters.Add(SqlParameterHelper.Int("@IdRol", dto.IdRol));
                cmd.Parameters.Add(SqlParameterHelper.Int("@Id", dto.IdUsuario));
                return await cmd.ExecuteNonQueryAsync() > 0;
            });
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await WithConnectionAsync(async conn =>
            {
                using var cmd = new SqlCommand(
                    "DELETE FROM Usuarios WHERE IdUsuario = @Id", conn);
                cmd.Parameters.Add(SqlParameterHelper.Int("@Id", id));
                return await cmd.ExecuteNonQueryAsync() > 0;
            });
        }

        public async Task<bool> IsAdminAsync(int id)
        {
            return await WithConnectionAsync(async conn =>
            {
                using var cmd = new SqlCommand(
                    "SELECT IdRol FROM Usuarios WHERE IdUsuario = @Id", conn);
                cmd.Parameters.Add(SqlParameterHelper.Int("@Id", id));
                var result = await cmd.ExecuteScalarAsync();
                return result != null && (int)result == 1;
            });
        }

        public async Task<int> CountClientesAsync()
        {
            return await WithConnectionAsync(async conn =>
            {
                using var cmd = new SqlCommand(
                    "SELECT COUNT(*) FROM Usuarios WHERE IdRol = 2", conn);
                var result = await cmd.ExecuteScalarAsync();
                return result != null ? (int)result : 0;
            });
        }

        private static PerfilDto MapPerfil(SqlDataReader reader)
        {
            return new PerfilDto
            {
                IdUsuario = (int)reader["IdUsuario"],
                Nombre = reader["Nombre"].ToString()!,
                Email = reader["Email"].ToString()!,
                Telefono = reader["Telefono"] == DBNull.Value ? "" : reader["Telefono"].ToString()!,
                Direccion = reader["Direccion"] == DBNull.Value ? "" : reader["Direccion"].ToString()!,
                Ciudad = reader["Ciudad"] == DBNull.Value ? "" : reader["Ciudad"].ToString()!,
                CodigoPostal = reader["CodigoPostal"] == DBNull.Value ? "" : reader["CodigoPostal"].ToString()!,
                FotoPerfil = reader["FotoPerfil"] == DBNull.Value ? "" : reader["FotoPerfil"].ToString()!,
                FechaRegistro = (DateTime)reader["FechaRegistro"]
            };
        }
    }
}
