using Microsoft.Data.SqlClient;
using EnerGym.Models;
using EnerGym.Repositories.Interfaces;
using EnerGym.Infrastructure;

namespace EnerGym.Repositories.Implementations
{
    public class AuthRepository : BaseRepository, IAuthRepository
    {
        public AuthRepository(Database db) : base(db) { }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await WithConnectionAsync(async conn =>
            {
                using var cmd = new SqlCommand(
                    "SELECT COUNT(*) FROM Usuarios WHERE Email = @Email", conn);
                cmd.Parameters.Add(SqlParameterHelper.String("@Email", 150, email.ToLower()));
                var result = await cmd.ExecuteScalarAsync();
                return result != null && (int)result > 0;
            });
        }

        public async Task RegisterAsync(RegisterDto dto)
        {
            await WithConnectionAsync(async conn =>
            {
                string hash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
                using var cmd = new SqlCommand(
                    @"INSERT INTO Usuarios (Nombre, Email, PasswordHash, IdRol, FechaRegistro)
                      VALUES (@Nombre, @Email, @PasswordHash, 2, GETDATE())", conn);
                cmd.Parameters.Add(SqlParameterHelper.String("@Nombre", 100, dto.Nombre));
                cmd.Parameters.Add(SqlParameterHelper.String("@Email", 150, dto.Email.ToLower()));
                cmd.Parameters.Add(SqlParameterHelper.String("@PasswordHash", 255, hash));
                await cmd.ExecuteNonQueryAsync();
            });
        }

        public async Task<LoginResponse?> LoginAsync(string email)
        {
            return await WithConnectionAsync(async conn =>
            {
                using var cmd = new SqlCommand(
                    @"SELECT IdUsuario, Nombre, Email, PasswordHash, IdRol
                      FROM Usuarios WHERE Email = @Email", conn);
                cmd.Parameters.Add(SqlParameterHelper.String("@Email", 150, email.ToLower()));

                using var reader = await cmd.ExecuteReaderAsync();
                if (!await reader.ReadAsync()) return null;

                return new LoginResponse
                {
                    IdUsuario = (int)reader["IdUsuario"],
                    Nombre = reader["Nombre"].ToString()!,
                    Email = reader["Email"].ToString()!,
                    PasswordHash = reader["PasswordHash"].ToString()!,
                    IdRol = (int)reader["IdRol"]
                };
            });
        }
    }
}
