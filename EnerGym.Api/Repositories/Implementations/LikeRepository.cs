using Microsoft.Data.SqlClient;
using EnerGym.Infrastructure;
using EnerGym.Repositories.Interfaces;

namespace EnerGym.Repositories.Implementations
{
    public class LikeRepository : BaseRepository, ILikeRepository
    {
        public LikeRepository(Database db) : base(db) { }

        public async Task<bool> ToggleAsync(int idUsuario, int idProducto)
        {
            return await WithConnectionAsync(async conn =>
            {
                int? idLike = null;
                using (var cmd = new SqlCommand(
                    "SELECT IdLike FROM LikesProductos WHERE IdUsuario = @IdUsuario AND IdProducto = @IdProducto", conn))
                {
                    cmd.Parameters.Add(SqlParameterHelper.Int("@IdUsuario", idUsuario));
                    cmd.Parameters.Add(SqlParameterHelper.Int("@IdProducto", idProducto));
                    var result = await cmd.ExecuteScalarAsync();
                    if (result != null && result != DBNull.Value)
                    {
                        idLike = (int)result;
                    }
                }

                if (idLike.HasValue)
                {
                    using var cmd = new SqlCommand("DELETE FROM LikesProductos WHERE IdLike = @IdLike", conn);
                    cmd.Parameters.Add(SqlParameterHelper.Int("@IdLike", idLike.Value));
                    await cmd.ExecuteNonQueryAsync();
                    return false;
                }
                else
                {
                    using var cmd = new SqlCommand(
                        "INSERT INTO LikesProductos (IdUsuario, IdProducto) VALUES (@IdUsuario, @IdProducto)", conn);
                    cmd.Parameters.Add(SqlParameterHelper.Int("@IdUsuario", idUsuario));
                    cmd.Parameters.Add(SqlParameterHelper.Int("@IdProducto", idProducto));
                    await cmd.ExecuteNonQueryAsync();
                    return true;
                }
            });
        }

        public async Task<int> CountByProductoAsync(int idProducto)
        {
            return await WithConnectionAsync(async conn =>
            {
                using var cmd = new SqlCommand("SELECT COUNT(*) FROM LikesProductos WHERE IdProducto = @IdProducto", conn);
                cmd.Parameters.Add(SqlParameterHelper.Int("@IdProducto", idProducto));
                var result = await cmd.ExecuteScalarAsync();
                if (result == null || result == DBNull.Value)
                    return 0;
                return (int)result;
            });
        }

        public async Task<List<ProductoLikeDto>> GetByUsuarioAsync(int idUsuario)
        {
            return await WithConnectionAsync(async conn =>
            {
                var list = new List<ProductoLikeDto>();
                using var cmd = new SqlCommand(
                    "SELECT p.IdProducto, p.Nombre, p.Precio, p.Stock, p.Imagen " +
                    "FROM LikesProductos l INNER JOIN Productos p ON l.IdProducto = p.IdProducto " +
                    "WHERE l.IdUsuario = @IdUsuario ORDER BY l.IdLike DESC", conn);
                cmd.Parameters.Add(SqlParameterHelper.Int("@IdUsuario", idUsuario));
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    list.Add(new ProductoLikeDto
                    {
                        IdProducto = reader.GetInt32(0),
                        Nombre = reader.IsDBNull(1) ? "" : reader.GetString(1),
                        Precio = reader.GetDecimal(2),
                        Stock = reader.GetInt32(3),
                        Imagen = reader.IsDBNull(4) ? "" : reader.GetString(4),
                    });
                }
                return list;
            });
        }
    }
}
