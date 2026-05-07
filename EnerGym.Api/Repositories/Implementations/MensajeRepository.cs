using Microsoft.Data.SqlClient;
using EnerGym.Infrastructure;
using EnerGym.Repositories.Interfaces;

namespace EnerGym.Repositories.Implementations
{
    public class MensajeRepository : BaseRepository, IMensajeRepository
    {
        public MensajeRepository(Database db) : base(db) { }

        public async Task<int> CrearAsync(CrearMensajeDto dto)
        {
            return await WithConnectionAsync(async conn =>
            {
                using var cmd = new SqlCommand(
                    "INSERT INTO MensajesSoporte (IdUsuario, Nombre, Email, Asunto, Mensaje, Fecha, Leido, Respondido) " +
                    "OUTPUT INSERTED.IdMensaje " +
                    "VALUES (@IdUsuario, @Nombre, @Email, @Asunto, @Mensaje, GETDATE(), 0, 0)", conn);
                cmd.Parameters.Add(SqlParameterHelper.Int("@IdUsuario", dto.IdUsuario));
                cmd.Parameters.Add(SqlParameterHelper.String("@Nombre", 200, dto.Nombre));
                cmd.Parameters.Add(SqlParameterHelper.String("@Email", 200, dto.Email));
                cmd.Parameters.Add(SqlParameterHelper.String("@Asunto", 200, dto.Asunto));
                cmd.Parameters.Add(SqlParameterHelper.String("@Mensaje", 4000, dto.Mensaje));
                var result = await cmd.ExecuteScalarAsync();
                return result == null || result == DBNull.Value ? 0 : (int)result;
            });
        }

        public async Task<List<MensajeDto>> GetAllAsync()
        {
            return await WithConnectionAsync(async conn =>
            {
                var list = new List<MensajeDto>();
                using var cmd = new SqlCommand(
                    "SELECT IdMensaje, IdUsuario, Nombre, Email, Asunto, Mensaje, Fecha, Leido, Respondido, Respuesta " +
                    "FROM MensajesSoporte ORDER BY Fecha DESC", conn);
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    list.Add(MapMensaje(reader));
                }
                return list;
            });
        }

        public async Task<List<MensajeDto>> GetByUsuarioAsync(int idUsuario)
        {
            return await WithConnectionAsync(async conn =>
            {
                var list = new List<MensajeDto>();
                using var cmd = new SqlCommand(
                    "SELECT IdMensaje, Asunto, Mensaje, Fecha, Leido, Respondido, Respuesta " +
                    "FROM MensajesSoporte WHERE IdUsuario = @IdUsuario ORDER BY Fecha DESC", conn);
                cmd.Parameters.Add(SqlParameterHelper.Int("@IdUsuario", idUsuario));
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    list.Add(new MensajeDto
                    {
                        IdMensaje = reader.GetInt32(0),
                        IdUsuario = idUsuario,
                        Nombre = "",
                        Email = "",
                        Asunto = reader.IsDBNull(1) ? "" : reader.GetString(1),
                        Mensaje = reader.IsDBNull(2) ? "" : reader.GetString(2),
                        Fecha = reader.IsDBNull(3) ? "" : reader.GetDateTime(3).ToString("yyyy-MM-dd HH:mm:ss"),
                        Leido = reader.GetBoolean(4),
                        Respondido = reader.GetBoolean(5),
                        Respuesta = reader.IsDBNull(6) ? null : reader.GetString(6),
                    });
                }
                return list;
            });
        }

        public async Task<bool> ResponderAsync(int idMensaje, string? respuesta)
        {
            return await WithConnectionAsync(async conn =>
            {
                using var cmd = new SqlCommand(
                    "UPDATE MensajesSoporte SET Respuesta = @Respuesta, Respondido = 1 WHERE IdMensaje = @IdMensaje", conn);
                cmd.Parameters.Add(SqlParameterHelper.String("@Respuesta", 4000, respuesta));
                cmd.Parameters.Add(SqlParameterHelper.Int("@IdMensaje", idMensaje));
                var rows = await cmd.ExecuteNonQueryAsync();
                return rows > 0;
            });
        }

        public async Task<bool> MarcarLeidoAsync(int idMensaje)
        {
            return await WithConnectionAsync(async conn =>
            {
                using var cmd = new SqlCommand("UPDATE MensajesSoporte SET Leido = 1 WHERE IdMensaje = @IdMensaje", conn);
                cmd.Parameters.Add(SqlParameterHelper.Int("@IdMensaje", idMensaje));
                var rows = await cmd.ExecuteNonQueryAsync();
                return rows > 0;
            });
        }

        public async Task<bool> MarcarRespondidoAsync(int idMensaje)
        {
            return await WithConnectionAsync(async conn =>
            {
                using var cmd = new SqlCommand("UPDATE MensajesSoporte SET Respondido = 1 WHERE IdMensaje = @IdMensaje", conn);
                cmd.Parameters.Add(SqlParameterHelper.Int("@IdMensaje", idMensaje));
                var rows = await cmd.ExecuteNonQueryAsync();
                return rows > 0;
            });
        }

        public async Task<bool> EliminarAsync(int idMensaje)
        {
            return await WithConnectionAsync(async conn =>
            {
                using var cmd = new SqlCommand("DELETE FROM MensajesSoporte WHERE IdMensaje = @IdMensaje", conn);
                cmd.Parameters.Add(SqlParameterHelper.Int("@IdMensaje", idMensaje));
                var rows = await cmd.ExecuteNonQueryAsync();
                return rows > 0;
            });
        }

        public async Task<int> CountNoLeidosAsync()
        {
            return await WithConnectionAsync(async conn =>
            {
                using var cmd = new SqlCommand("SELECT COUNT(*) FROM MensajesSoporte WHERE Leido = 0", conn);
                var result = await cmd.ExecuteScalarAsync();
                if (result == null || result == DBNull.Value)
                    return 0;
                return (int)result;
            });
        }

        private static MensajeDto MapMensaje(SqlDataReader reader)
        {
            return new MensajeDto
            {
                IdMensaje = reader.GetInt32(0),
                IdUsuario = reader.IsDBNull(1) ? null : reader.GetInt32(1),
                Nombre = reader.IsDBNull(2) ? "" : reader.GetString(2),
                Email = reader.IsDBNull(3) ? "" : reader.GetString(3),
                Asunto = reader.IsDBNull(4) ? "" : reader.GetString(4),
                Mensaje = reader.IsDBNull(5) ? "" : reader.GetString(5),
                Fecha = reader.IsDBNull(6) ? "" : reader.GetDateTime(6).ToString("yyyy-MM-dd HH:mm:ss"),
                Leido = reader.GetBoolean(7),
                Respondido = reader.GetBoolean(8),
                Respuesta = reader.IsDBNull(9) ? null : reader.GetString(9),
            };
        }
    }
}
