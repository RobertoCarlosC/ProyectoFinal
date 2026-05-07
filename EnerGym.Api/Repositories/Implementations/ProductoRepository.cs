using Microsoft.Data.SqlClient;
using EnerGym.Repositories.Interfaces;
using EnerGym.Infrastructure;

namespace EnerGym.Repositories.Implementations
{
    public class ProductoRepository : BaseRepository, IProductoRepository
    {
        public ProductoRepository(Database db) : base(db) { }

        public async Task<List<ProductoListDto>> GetAllAsync(int? idUsuario = null)
        {
            return await WithConnectionAsync(async conn =>
            {
                var list = new List<ProductoListDto>();
                using var cmd = new SqlCommand(@"
                    SELECT p.IdProducto, p.Nombre, p.Descripcion, p.Precio, p.Stock, p.Imagen, p.IdCategoria, c.Nombre AS NombreCategoria,
                        CASE WHEN EXISTS(SELECT 1 FROM LikesProductos WHERE IdProducto = p.IdProducto AND IdUsuario = @IdUsuario) THEN 1 ELSE 0 END AS TieneLike,
                        (SELECT COUNT(*) FROM LikesProductos WHERE IdProducto = p.IdProducto) AS TotalLikes
                    FROM Productos p
                    LEFT JOIN Categorias c ON p.IdCategoria = c.IdCategoria
                    ORDER BY p.IdProducto DESC", conn);
                cmd.Parameters.Add(SqlParameterHelper.Int("@IdUsuario", idUsuario));

                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    list.Add(MapProductoList(reader));
                }
                return list;
            });
        }

        public async Task<List<CategoriaDto>> GetCategoriasAsync()
        {
            return await WithConnectionAsync(async conn =>
            {
                var list = new List<CategoriaDto>();
                using var cmd = new SqlCommand(
                    "SELECT IdCategoria, Nombre, Descripcion FROM Categorias ORDER BY Nombre", conn);
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    list.Add(new CategoriaDto
                    {
                        IdCategoria = (int)reader["IdCategoria"],
                        Nombre = reader["Nombre"].ToString()!,
                        Descripcion = reader["Descripcion"] == DBNull.Value ? "" : reader["Descripcion"].ToString()!
                    });
                }
                return list;
            });
        }

        public async Task<ProductoDetailDto?> GetByIdAsync(int id, int? idUsuario = null)
        {
            return await WithConnectionAsync(async conn =>
            {
                using var cmd = new SqlCommand(@"
                    SELECT p.IdProducto, p.Nombre, p.Descripcion, p.Precio, p.Stock, p.Imagen, p.IdCategoria, c.Nombre AS NombreCategoria,
                        CASE WHEN EXISTS(SELECT 1 FROM LikesProductos WHERE IdProducto = p.IdProducto AND IdUsuario = @IdUsuario) THEN 1 ELSE 0 END AS TieneLike,
                        (SELECT COUNT(*) FROM LikesProductos WHERE IdProducto = p.IdProducto) AS TotalLikes
                    FROM Productos p
                    LEFT JOIN Categorias c ON p.IdCategoria = c.IdCategoria
                    WHERE p.IdProducto = @Id", conn);
                cmd.Parameters.Add(SqlParameterHelper.Int("@Id", id));
                cmd.Parameters.Add(SqlParameterHelper.Int("@IdUsuario", idUsuario));

                using var reader = await cmd.ExecuteReaderAsync();
                if (!await reader.ReadAsync()) return null;

                var producto = new ProductoDetailDto();
                MapProductoList(reader, producto);
                await reader.CloseAsync();

                using var imgCmd = new SqlCommand(
                    "SELECT IdImagen, UrlImagen, Orden FROM ProductoImagenes WHERE IdProducto = @Id ORDER BY Orden", conn);
                imgCmd.Parameters.Add(SqlParameterHelper.Int("@Id", id));
                using var imgReader = await imgCmd.ExecuteReaderAsync();
                while (await imgReader.ReadAsync())
                {
                    producto.Imagenes.Add(new ProductoImagenDto
                    {
                        IdImagen = (int)imgReader["IdImagen"],
                        UrlImagen = imgReader["UrlImagen"].ToString()!,
                        Orden = (int)imgReader["Orden"]
                    });
                }
                return producto;
            });
        }

        public async Task<int> CreateAsync(EnerGym.Models.ProductoDto dto)
        {
            return await WithConnectionAsync(async conn =>
            {
                using var cmd = new SqlCommand(@"
                    INSERT INTO Productos (Nombre, Descripcion, Precio, Stock, Imagen, IdCategoria)
                    OUTPUT INSERTED.IdProducto
                    VALUES (@Nombre, @Descripcion, @Precio, @Stock, @Imagen, @IdCategoria)", conn);
                cmd.Parameters.Add(SqlParameterHelper.String("@Nombre", 150, dto.Nombre));
                cmd.Parameters.Add(SqlParameterHelper.String("@Descripcion", 1000, dto.Descripcion));
                cmd.Parameters.Add(SqlParameterHelper.Decimal("@Precio", dto.Precio));
                cmd.Parameters.Add(SqlParameterHelper.Int("@Stock", dto.Stock));
                cmd.Parameters.Add(SqlParameterHelper.String("@Imagen", 500, dto.Imagen));
                cmd.Parameters.Add(SqlParameterHelper.Int("@IdCategoria", dto.IdCategoria));
                var result = await cmd.ExecuteScalarAsync();
                return result != null ? (int)result : 0;
            });
        }

        public async Task<bool> UpdateAsync(int id, EnerGym.Models.ProductoDto dto)
        {
            return await WithConnectionAsync(async conn =>
            {
                using var cmd = new SqlCommand(@"
                    UPDATE Productos
                    SET Nombre = @Nombre, Descripcion = @Descripcion, Precio = @Precio,
                        Stock = @Stock, Imagen = @Imagen, IdCategoria = @IdCategoria
                    WHERE IdProducto = @Id", conn);
                cmd.Parameters.Add(SqlParameterHelper.Int("@Id", id));
                cmd.Parameters.Add(SqlParameterHelper.String("@Nombre", 150, dto.Nombre));
                cmd.Parameters.Add(SqlParameterHelper.String("@Descripcion", 1000, dto.Descripcion));
                cmd.Parameters.Add(SqlParameterHelper.Decimal("@Precio", dto.Precio));
                cmd.Parameters.Add(SqlParameterHelper.Int("@Stock", dto.Stock));
                cmd.Parameters.Add(SqlParameterHelper.String("@Imagen", 500, dto.Imagen));
                cmd.Parameters.Add(SqlParameterHelper.Int("@IdCategoria", dto.IdCategoria));
                return await cmd.ExecuteNonQueryAsync() > 0;
            });
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await WithConnectionAsync(async conn =>
            {
                using (var delImgs = new SqlCommand("DELETE FROM ProductoImagenes WHERE IdProducto = @Id", conn))
                {
                    delImgs.Parameters.Add(SqlParameterHelper.Int("@Id", id));
                    await delImgs.ExecuteNonQueryAsync();
                }
                using (var delLikes = new SqlCommand("DELETE FROM LikesProductos WHERE IdProducto = @Id", conn))
                {
                    delLikes.Parameters.Add(SqlParameterHelper.Int("@Id", id));
                    await delLikes.ExecuteNonQueryAsync();
                }
                using (var delCart = new SqlCommand("DELETE FROM CarritoProductos WHERE IdProducto = @Id", conn))
                {
                    delCart.Parameters.Add(SqlParameterHelper.Int("@Id", id));
                    await delCart.ExecuteNonQueryAsync();
                }
                using var cmd = new SqlCommand("DELETE FROM Productos WHERE IdProducto = @Id", conn);
                cmd.Parameters.Add(SqlParameterHelper.Int("@Id", id));
                return await cmd.ExecuteNonQueryAsync() > 0;
            });
        }

        public async Task<int> CountAsync()
        {
            return await WithConnectionAsync(async conn =>
            {
                using var cmd = new SqlCommand("SELECT COUNT(*) FROM Productos", conn);
                var result = await cmd.ExecuteScalarAsync();
                return result != null ? (int)result : 0;
            });
        }

        public async Task<int> CountSinStockAsync()
        {
            return await WithConnectionAsync(async conn =>
            {
                using var cmd = new SqlCommand("SELECT COUNT(*) FROM Productos WHERE Stock = 0", conn);
                var result = await cmd.ExecuteScalarAsync();
                return result != null ? (int)result : 0;
            });
        }

        private static ProductoListDto MapProductoList(SqlDataReader reader)
        {
            var dto = new ProductoListDto();
            MapProductoList(reader, dto);
            return dto;
        }

        private static void MapProductoList(SqlDataReader reader, ProductoListDto dto)
        {
            dto.IdProducto = (int)reader["IdProducto"];
            dto.Nombre = reader["Nombre"].ToString()!;
            dto.Descripcion = reader["Descripcion"] == DBNull.Value ? "" : reader["Descripcion"].ToString()!;
            dto.Precio = Convert.ToDecimal(reader["Precio"]);
            dto.Stock = (int)reader["Stock"];
            dto.Imagen = reader["Imagen"] == DBNull.Value ? "" : reader["Imagen"].ToString()!;
            dto.IdCategoria = (int)reader["IdCategoria"];
            dto.NombreCategoria = reader["NombreCategoria"] == DBNull.Value ? "" : reader["NombreCategoria"].ToString()!;
            dto.TieneLike = (int)reader["TieneLike"] == 1;
            dto.TotalLikes = (int)reader["TotalLikes"];
        }
    }
}
