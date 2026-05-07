using Microsoft.Data.SqlClient;
using EnerGym.Repositories.Interfaces;
using EnerGym.Infrastructure;

namespace EnerGym.Repositories.Implementations
{
    public class CarritoRepository : BaseRepository, ICarritoRepository
    {
        public CarritoRepository(Database db) : base(db) { }

        public async Task<CarritoDto?> GetCarritoAsync(int idUsuario)
        {
            return await WithConnectionAsync(async conn =>
            {
                int idCarrito = await ObtenerOCrearCarritoAsync(conn, idUsuario);

                var dto = new CarritoDto { IdCarrito = idCarrito };
                using var cmd = new SqlCommand(@"
                    SELECT cp.Id, cp.IdCarrito, cp.IdProducto, cp.Cantidad, p.Nombre AS NombreProducto, p.Imagen, p.Precio
                    FROM CarritoProductos cp
                    INNER JOIN Productos p ON cp.IdProducto = p.IdProducto
                    WHERE cp.IdCarrito = @IdCarrito", conn);
                cmd.Parameters.Add(SqlParameterHelper.Int("@IdCarrito", idCarrito));

                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    decimal precio = Convert.ToDecimal(reader["Precio"]);
                    int cantidad = (int)reader["Cantidad"];
                    decimal subtotal = precio * cantidad;
                    dto.Total += subtotal;

                    dto.Items.Add(new CarritoItemDto
                    {
                        Id = (int)reader["Id"],
                        IdCarrito = (int)reader["IdCarrito"],
                        IdProducto = (int)reader["IdProducto"],
                        NombreProducto = reader["NombreProducto"].ToString()!,
                        Imagen = reader["Imagen"] == DBNull.Value ? "" : reader["Imagen"].ToString()!,
                        Precio = precio,
                        Cantidad = cantidad,
                        Subtotal = subtotal
                    });
                }
                return dto;
            });
        }

        public async Task<bool> AddItemAsync(EnerGym.Models.AddCarritoDto dto)
        {
            return await WithConnectionAsync(async conn =>
            {
                var stock = await GetStockAsync(conn, dto.IdProducto);
                if (!stock.HasValue) return false;

                int idCarrito = await ObtenerOCrearCarritoAsync(conn, dto.IdUsuario);

                using var checkCmd = new SqlCommand(
                    "SELECT Id, Cantidad FROM CarritoProductos WHERE IdCarrito = @IdCarrito AND IdProducto = @IdProducto", conn);
                checkCmd.Parameters.Add(SqlParameterHelper.Int("@IdCarrito", idCarrito));
                checkCmd.Parameters.Add(SqlParameterHelper.Int("@IdProducto", dto.IdProducto));

                using var reader = await checkCmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    int itemId = (int)reader["Id"];
                    int cantidadActual = (int)reader["Cantidad"];
                    await reader.CloseAsync();

                    int nuevaCantidad = cantidadActual + dto.Cantidad;
                    if (nuevaCantidad > stock.Value) return false;

                    using var upd = new SqlCommand(
                        "UPDATE CarritoProductos SET Cantidad = @Cantidad WHERE Id = @Id", conn);
                    upd.Parameters.Add(SqlParameterHelper.Int("@Cantidad", nuevaCantidad));
                    upd.Parameters.Add(SqlParameterHelper.Int("@Id", itemId));
                    return await upd.ExecuteNonQueryAsync() > 0;
                }
                else
                {
                    await reader.CloseAsync();
                    if (dto.Cantidad > stock.Value) return false;

                    using var ins = new SqlCommand(
                        "INSERT INTO CarritoProductos (IdCarrito, IdProducto, Cantidad) VALUES (@IdCarrito, @IdProducto, @Cantidad)", conn);
                    ins.Parameters.Add(SqlParameterHelper.Int("@IdCarrito", idCarrito));
                    ins.Parameters.Add(SqlParameterHelper.Int("@IdProducto", dto.IdProducto));
                    ins.Parameters.Add(SqlParameterHelper.Int("@Cantidad", dto.Cantidad));
                    return await ins.ExecuteNonQueryAsync() > 0;
                }
            });
        }

        public async Task<bool> UpdateCantidadAsync(int idItem, int cantidad)
        {
            return await WithConnectionAsync(async conn =>
            {
                if (cantidad <= 0)
                {
                    using var del = new SqlCommand("DELETE FROM CarritoProductos WHERE Id = @Id", conn);
                    del.Parameters.Add(SqlParameterHelper.Int("@Id", idItem));
                    return await del.ExecuteNonQueryAsync() > 0;
                }

                using var stockCmd = new SqlCommand(@"
                    SELECT cp.IdProducto, p.Stock FROM CarritoProductos cp
                    INNER JOIN Productos p ON cp.IdProducto = p.IdProducto
                    WHERE cp.Id = @Id", conn);
                stockCmd.Parameters.Add(SqlParameterHelper.Int("@Id", idItem));
                using var r = await stockCmd.ExecuteReaderAsync();
                if (!await r.ReadAsync()) return false;
                int stock = (int)r["Stock"];
                await r.CloseAsync();

                if (cantidad > stock) return false;

                using var cmd = new SqlCommand(
                    "UPDATE CarritoProductos SET Cantidad = @Cantidad WHERE Id = @Id", conn);
                cmd.Parameters.Add(SqlParameterHelper.Int("@Cantidad", cantidad));
                cmd.Parameters.Add(SqlParameterHelper.Int("@Id", idItem));
                return await cmd.ExecuteNonQueryAsync() > 0;
            });
        }

        public async Task<bool> RemoveItemAsync(int idItem)
        {
            return await WithConnectionAsync(async conn =>
            {
                using var cmd = new SqlCommand("DELETE FROM CarritoProductos WHERE Id = @Id", conn);
                cmd.Parameters.Add(SqlParameterHelper.Int("@Id", idItem));
                return await cmd.ExecuteNonQueryAsync() > 0;
            });
        }

        public async Task<int?> GetStockAsync(int idProducto)
        {
            return await WithConnectionAsync(async conn => await GetStockAsync(conn, idProducto));
        }

        public async Task<int> GetCarritoIdAsync(int idUsuario)
        {
            return await WithConnectionAsync(async conn => await ObtenerOCrearCarritoAsync(conn, idUsuario));
        }

        public async Task<bool> ClearCarritoAsync(int idCarrito)
        {
            return await WithConnectionAsync(async conn =>
            {
                using var cmd = new SqlCommand("DELETE FROM CarritoProductos WHERE IdCarrito = @IdCarrito", conn);
                cmd.Parameters.Add(SqlParameterHelper.Int("@IdCarrito", idCarrito));
                return await cmd.ExecuteNonQueryAsync() >= 0;
            });
        }

        public async Task<bool> ClearCarritoByUsuarioAsync(int idUsuario)
        {
            return await WithConnectionAsync(async conn =>
            {
                int idCarrito = await ObtenerOCrearCarritoAsync(conn, idUsuario);
                return await ClearCarritoAsync(idCarrito);
            });
        }

        private static async Task<int> ObtenerOCrearCarritoAsync(SqlConnection conn, int idUsuario)
        {
            using var getCmd = new SqlCommand(
                "SELECT IdCarrito FROM Carritos WHERE IdUsuario = @IdUsuario", conn);
            getCmd.Parameters.Add(SqlParameterHelper.Int("@IdUsuario", idUsuario));
            var result = await getCmd.ExecuteScalarAsync();

            if (result != null && result != DBNull.Value)
                return (int)result;

            using var insertCmd = new SqlCommand(
                "INSERT INTO Carritos (IdUsuario, FechaCreacion) OUTPUT INSERTED.IdCarrito VALUES (@IdUsuario, GETDATE())", conn);
            insertCmd.Parameters.Add(SqlParameterHelper.Int("@IdUsuario", idUsuario));
            var inserted = await insertCmd.ExecuteScalarAsync();
            return (int)inserted!;
        }

        private static async Task<int?> GetStockAsync(SqlConnection conn, int idProducto)
        {
            using var cmd = new SqlCommand(
                "SELECT Stock FROM Productos WHERE IdProducto = @IdProducto", conn);
            cmd.Parameters.Add(SqlParameterHelper.Int("@IdProducto", idProducto));
            var result = await cmd.ExecuteScalarAsync();
            return result == null || result == DBNull.Value ? null : (int)result;
        }
    }
}
