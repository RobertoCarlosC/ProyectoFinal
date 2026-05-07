using Microsoft.Data.SqlClient;
using EnerGym.Repositories.Interfaces;
using EnerGym.Infrastructure;

namespace EnerGym.Repositories.Implementations
{
    public class PedidoRepository : BaseRepository, IPedidoRepository
    {
        public PedidoRepository(Database db) : base(db) { }

        public async Task<PedidoConfirmadoDto?> ConfirmarAsync(EnerGym.Models.ConfirmarPedidoDto dto)
        {
            using var conn = GetConnection();
            await conn.OpenAsync();
            using var transaction = conn.BeginTransaction();

            try
            {
                var carritoCmd = new SqlCommand(
                    "SELECT IdCarrito FROM Carritos WHERE IdUsuario = @IdUsuario", conn, transaction);
                carritoCmd.Parameters.Add(SqlParameterHelper.Int("@IdUsuario", dto.IdUsuario));
                var carritoResult = await carritoCmd.ExecuteScalarAsync();

                if (carritoResult == null || carritoResult == DBNull.Value)
                    return null;

                int idCarrito = (int)carritoResult;

                var itemsCmd = new SqlCommand(@"
                    SELECT cp.IdProducto, cp.Cantidad, p.Precio, p.Stock, p.Nombre
                    FROM CarritoProductos cp
                    INNER JOIN Productos p ON cp.IdProducto = p.IdProducto
                    WHERE cp.IdCarrito = @IdCarrito", conn, transaction);
                itemsCmd.Parameters.Add(SqlParameterHelper.Int("@IdCarrito", idCarrito));

                var items = new List<(int idProducto, int cantidad, decimal precio, int stock, string nombre)>();
                using (var reader = await itemsCmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        items.Add(((int)reader["IdProducto"], (int)reader["Cantidad"],
                            Convert.ToDecimal(reader["Precio"]), (int)reader["Stock"], reader["Nombre"].ToString()!));
                    }
                }

                if (items.Count == 0) return null;

                foreach (var item in items)
                {
                    if (item.stock < item.cantidad)
                        throw new InvalidOperationException($"Stock insuficiente para '{item.nombre}'");
                }

                decimal total = items.Sum(i => i.precio * i.cantidad);

                var pedidoCmd = new SqlCommand(@"
                    INSERT INTO Pedidos (IdUsuario, Fecha, Total, Estado, DireccionEnvio, MetodoPago)
                    OUTPUT INSERTED.IdPedido
                    VALUES (@IdUsuario, GETDATE(), @Total, 'Pendiente', @DireccionEnvio, @MetodoPago)", conn, transaction);
                pedidoCmd.Parameters.Add(SqlParameterHelper.Int("@IdUsuario", dto.IdUsuario));
                pedidoCmd.Parameters.Add(SqlParameterHelper.Decimal("@Total", total));
                pedidoCmd.Parameters.Add(SqlParameterHelper.String("@DireccionEnvio", 500, dto.DireccionEnvio));
                pedidoCmd.Parameters.Add(SqlParameterHelper.String("@MetodoPago", 50, dto.MetodoPago));
                int idPedido = (int)(await pedidoCmd.ExecuteScalarAsync())!;

                foreach (var item in items)
                {
                    var detalleCmd = new SqlCommand(@"
                        INSERT INTO PedidoProductos (IdPedido, IdProducto, Cantidad, Precio)
                        VALUES (@IdPedido, @IdProducto, @Cantidad, @Precio)", conn, transaction);
                    detalleCmd.Parameters.Add(SqlParameterHelper.Int("@IdPedido", idPedido));
                    detalleCmd.Parameters.Add(SqlParameterHelper.Int("@IdProducto", item.idProducto));
                    detalleCmd.Parameters.Add(SqlParameterHelper.Int("@Cantidad", item.cantidad));
                    detalleCmd.Parameters.Add(SqlParameterHelper.Decimal("@Precio", item.precio));
                    await detalleCmd.ExecuteNonQueryAsync();

                    var stockCmd = new SqlCommand(
                        "UPDATE Productos SET Stock = Stock - @Cantidad WHERE IdProducto = @IdProducto", conn, transaction);
                    stockCmd.Parameters.Add(SqlParameterHelper.Int("@Cantidad", item.cantidad));
                    stockCmd.Parameters.Add(SqlParameterHelper.Int("@IdProducto", item.idProducto));
                    await stockCmd.ExecuteNonQueryAsync();
                }

                var clearCmd = new SqlCommand(
                    "DELETE FROM CarritoProductos WHERE IdCarrito = @IdCarrito", conn, transaction);
                clearCmd.Parameters.Add(SqlParameterHelper.Int("@IdCarrito", idCarrito));
                await clearCmd.ExecuteNonQueryAsync();

                transaction.Commit();
                return new PedidoConfirmadoDto { IdPedido = idPedido, Total = total };
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<PedidoDetalleDto?> GetByIdAsync(int idPedido)
        {
            return await WithConnectionAsync(async conn =>
            {
                using var pedidoCmd = new SqlCommand(
                    "SELECT IdPedido, Fecha, Total, Estado, DireccionEnvio, MetodoPago FROM Pedidos WHERE IdPedido = @Id", conn);
                pedidoCmd.Parameters.Add(SqlParameterHelper.Int("@Id", idPedido));

                PedidoDetalleDto? dto = null;
                using (var r = await pedidoCmd.ExecuteReaderAsync())
                {
                    if (await r.ReadAsync())
                    {
                        dto = new PedidoDetalleDto
                        {
                            IdPedido = (int)r["IdPedido"],
                            Fecha = (DateTime)r["Fecha"],
                            Total = Convert.ToDecimal(r["Total"]),
                            Estado = r["Estado"].ToString()!,
                            DireccionEnvio = r["DireccionEnvio"] == DBNull.Value ? null : r["DireccionEnvio"].ToString(),
                            MetodoPago = r["MetodoPago"] == DBNull.Value ? null : r["MetodoPago"].ToString()
                        };
                    }
                }

                if (dto == null) return null;

                using var cmd = new SqlCommand(@"
                    SELECT pp.IdDetalle, pp.IdProducto, pp.Cantidad, pp.Precio, p.Nombre AS NombreProducto, p.Imagen AS ImagenProducto
                    FROM PedidoProductos pp
                    INNER JOIN Productos p ON pp.IdProducto = p.IdProducto
                    WHERE pp.IdPedido = @IdPedido", conn);
                cmd.Parameters.Add(SqlParameterHelper.Int("@IdPedido", idPedido));
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    dto.Productos.Add(new PedidoProductoDto
                    {
                        IdDetalle = (int)reader["IdDetalle"],
                        IdProducto = (int)reader["IdProducto"],
                        NombreProducto = reader["NombreProducto"].ToString()!,
                        Imagen = reader["ImagenProducto"] == DBNull.Value ? "" : reader["ImagenProducto"].ToString()!,
                        Cantidad = (int)reader["Cantidad"],
                        Precio = Convert.ToDecimal(reader["Precio"]),
                        Subtotal = (int)reader["Cantidad"] * Convert.ToDecimal(reader["Precio"])
                    });
                }
                return dto;
            });
        }

        public async Task<List<PedidoResumenDto>> GetByUsuarioAsync(int idUsuario)
        {
            return await WithConnectionAsync(async conn =>
            {
                var list = new List<PedidoResumenDto>();
                using var cmd = new SqlCommand(
                    "SELECT IdPedido, Fecha, Total, Estado FROM Pedidos WHERE IdUsuario = @IdUsuario ORDER BY Fecha DESC", conn);
                cmd.Parameters.Add(SqlParameterHelper.Int("@IdUsuario", idUsuario));
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    list.Add(new PedidoResumenDto
                    {
                        IdPedido = (int)reader["IdPedido"],
                        Fecha = (DateTime)reader["Fecha"],
                        Total = Convert.ToDecimal(reader["Total"]),
                        Estado = reader["Estado"].ToString()!
                    });
                }
                return list;
            });
        }

        public async Task<List<PedidoAdminDto>> GetAllAsync(string? estado = null, int? idUsuario = null)
        {
            return await WithConnectionAsync(async conn =>
            {
                string query = @"
                    SELECT p.IdPedido, p.IdUsuario, u.Nombre AS UsuarioNombre, u.Email, p.Fecha, p.Total, p.Estado, COUNT(pp.IdDetalle) AS CantidadProductos
                    FROM Pedidos p
                    INNER JOIN Usuarios u ON p.IdUsuario = u.IdUsuario
                    LEFT JOIN PedidoProductos pp ON p.IdPedido = pp.IdPedido
                    WHERE 1=1";

                if (!string.IsNullOrEmpty(estado)) query += " AND p.Estado = @Estado";
                if (idUsuario.HasValue) query += " AND p.IdUsuario = @IdUsuario";
                query += " GROUP BY p.IdPedido, p.IdUsuario, u.Nombre, u.Email, p.Fecha, p.Total, p.Estado ORDER BY p.Fecha DESC";

                using var cmd = new SqlCommand(query, conn);
                if (!string.IsNullOrEmpty(estado))
                    cmd.Parameters.Add(SqlParameterHelper.String("@Estado", 50, estado));
                if (idUsuario.HasValue)
                    cmd.Parameters.Add(SqlParameterHelper.Int("@IdUsuario", idUsuario.Value));

                var list = new List<PedidoAdminDto>();
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    list.Add(new PedidoAdminDto
                    {
                        IdPedido = (int)reader["IdPedido"],
                        IdUsuario = (int)reader["IdUsuario"],
                        UsuarioNombre = reader["UsuarioNombre"].ToString()!,
                        UsuarioEmail = reader["Email"].ToString()!,
                        Fecha = (DateTime)reader["Fecha"],
                        Total = Convert.ToDecimal(reader["Total"]),
                        Estado = reader["Estado"].ToString()!,
                        CantidadProductos = (int)reader["CantidadProductos"]
                    });
                }
                return list;
            });
        }

        public async Task<EstadisticasDto> GetEstadisticasAsync()
        {
            return await WithConnectionAsync(async conn =>
            {
                using var cmd = new SqlCommand(@"
                    SELECT COUNT(*) as TotalPedidos,
                        SUM(CASE WHEN Estado = 'Pendiente' THEN 1 ELSE 0 END) as Pendientes,
                        SUM(CASE WHEN Estado = 'Procesando' THEN 1 ELSE 0 END) as EnProceso,
                        SUM(CASE WHEN Estado = 'Enviado' OR Estado = 'En reparto' THEN 1 ELSE 0 END) as Enviados,
                        SUM(CASE WHEN Estado = 'Entregado' THEN 1 ELSE 0 END) as Entregados,
                        SUM(Total) as VentasTotal
                    FROM Pedidos", conn);

                using var reader = await cmd.ExecuteReaderAsync();
                await reader.ReadAsync();
                int total = reader.IsDBNull(0) ? 0 : (int)reader["TotalPedidos"];
                decimal suma = reader.IsDBNull(5) ? 0 : Convert.ToDecimal(reader["VentasTotal"]);

                return new EstadisticasDto
                {
                    TotalPedidos = total,
                    Pendientes = reader.IsDBNull(1) ? 0 : (int)reader["Pendientes"],
                    EnProceso = reader.IsDBNull(2) ? 0 : (int)reader["EnProceso"],
                    Enviados = reader.IsDBNull(3) ? 0 : (int)reader["Enviados"],
                    Entregados = reader.IsDBNull(4) ? 0 : (int)reader["Entregados"],
                    VentasTotal = suma,
                    PromedioVenta = total > 0 ? suma / total : 0
                };
            });
        }

        public async Task<List<VentaDiaDto>> GetVentasPorDiaAsync(int dias)
        {
            return await WithConnectionAsync(async conn =>
            {
                using var cmd = new SqlCommand(@"
                    WITH Dias AS (
                        SELECT CAST(GETDATE() AS DATE) AS Fecha
                        UNION ALL
                        SELECT DATEADD(day, -1, Fecha)
                        FROM Dias
                        WHERE Fecha > DATEADD(day, -@Dias, CAST(GETDATE() AS DATE))
                    )
                    SELECT d.Fecha, COUNT(p.IdPedido) as TotalPedidos, ISNULL(SUM(p.Total), 0) as Ventas
                    FROM Dias d
                    LEFT JOIN Pedidos p ON CAST(p.Fecha AS DATE) = d.Fecha
                    GROUP BY d.Fecha
                    ORDER BY d.Fecha ASC
                    OPTION (MAXRECURSION 0)", conn);
                cmd.Parameters.Add(SqlParameterHelper.Int("@Dias", dias));

                var list = new List<VentaDiaDto>();
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    list.Add(new VentaDiaDto
                    {
                        Fecha = ((DateTime)reader["Fecha"]).ToString("yyyy-MM-dd"),
                        TotalPedidos = (int)reader["TotalPedidos"],
                        Ventas = Convert.ToDecimal(reader["Ventas"])
                    });
                }
                return list;
            });
        }

        public async Task<bool> CambiarEstadoAsync(int idPedido, string nuevoEstado, int idAdmin, string? notas)
        {
            using var conn = GetConnection();
            await conn.OpenAsync();

            using var getCmd = new SqlCommand(
                "SELECT Estado FROM Pedidos WHERE IdPedido = @Id", conn);
            getCmd.Parameters.Add(SqlParameterHelper.Int("@Id", idPedido));
            var estadoActual = (await getCmd.ExecuteScalarAsync())?.ToString();
            if (estadoActual == null) return false;

            using var updCmd = new SqlCommand(
                "UPDATE Pedidos SET Estado = @Estado, FechaActualizacion = GETDATE() WHERE IdPedido = @Id", conn);
            updCmd.Parameters.Add(SqlParameterHelper.String("@Estado", 50, nuevoEstado));
            updCmd.Parameters.Add(SqlParameterHelper.Int("@Id", idPedido));
            await updCmd.ExecuteNonQueryAsync();

            try
            {
                using var histCmd = new SqlCommand(@"
                    INSERT INTO PedidoHistorialEstados (IdPedido, EstadoAnterior, EstadoNuevo, Fecha, CambiadoPor, Notas)
                    VALUES (@IdPedido, @EstadoAnterior, @EstadoNuevo, GETDATE(), @CambiadoPor, @Notas)", conn);
                histCmd.Parameters.Add(SqlParameterHelper.Int("@IdPedido", idPedido));
                histCmd.Parameters.Add(SqlParameterHelper.String("@EstadoAnterior", 50, estadoActual));
                histCmd.Parameters.Add(SqlParameterHelper.String("@EstadoNuevo", 50, nuevoEstado));
                histCmd.Parameters.Add(SqlParameterHelper.String("@CambiadoPor", 100, $"Admin_{idAdmin}"));
                histCmd.Parameters.Add(SqlParameterHelper.String("@Notas", 500, notas));
                await histCmd.ExecuteNonQueryAsync();
            }
            catch (SqlException ex) when (ex.Number == 208) { /* tabla no existe */ }

            return true;
        }

        public async Task<List<HistorialEstadoDto>> GetHistorialAsync(int idPedido)
        {
            return await WithConnectionAsync(async conn =>
            {
                var list = new List<HistorialEstadoDto>();
                try
                {
                    using var cmd = new SqlCommand(@"
                        SELECT IdHistorial, IdPedido, EstadoAnterior, EstadoNuevo, Fecha, CambiadoPor, Notas
                        FROM PedidoHistorialEstados
                        WHERE IdPedido = @IdPedido
                        ORDER BY Fecha DESC", conn);
                    cmd.Parameters.Add(SqlParameterHelper.Int("@IdPedido", idPedido));
                    using var reader = await cmd.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        list.Add(new HistorialEstadoDto
                        {
                            IdHistorial = (int)reader["IdHistorial"],
                            IdPedido = (int)reader["IdPedido"],
                            EstadoAnterior = reader["EstadoAnterior"] == DBNull.Value ? null : reader["EstadoAnterior"].ToString(),
                            EstadoNuevo = reader["EstadoNuevo"].ToString()!,
                            Fecha = (DateTime)reader["Fecha"],
                            CambiadoPor = reader["CambiadoPor"] == DBNull.Value ? null : reader["CambiadoPor"].ToString(),
                            Notas = reader["Notas"] == DBNull.Value ? null : reader["Notas"].ToString()
                        });
                    }
                }
                catch (SqlException ex) when (ex.Number == 208) { }
                return list;
            });
        }

        public async Task<bool> ConfirmarEntregaAsync(int idPedido, int idUsuario)
        {
            using var conn = GetConnection();
            await conn.OpenAsync();

            using var checkCmd = new SqlCommand(
                "SELECT IdUsuario, Estado FROM Pedidos WHERE IdPedido = @Id", conn);
            checkCmd.Parameters.Add(SqlParameterHelper.Int("@Id", idPedido));
            using var reader = await checkCmd.ExecuteReaderAsync();
            if (!await reader.ReadAsync()) return false;

            int idUsuarioPedido = (int)reader["IdUsuario"];
            string estadoActual = reader["Estado"].ToString() ?? "";
            await reader.CloseAsync();

            if (idUsuarioPedido != idUsuario) return false;
            if (estadoActual != "Enviado" && estadoActual != "En reparto") return false;

            using var upd = new SqlCommand(@"
                UPDATE Pedidos SET Estado = 'Entregado', FechaConfirmacionEntrega = GETDATE(), FechaActualizacion = GETDATE()
                WHERE IdPedido = @Id", conn);
            upd.Parameters.Add(SqlParameterHelper.Int("@Id", idPedido));
            await upd.ExecuteNonQueryAsync();

            try
            {
                using var hist = new SqlCommand(@"
                    INSERT INTO PedidoHistorialEstados (IdPedido, EstadoAnterior, EstadoNuevo, Fecha, CambiadoPor, Notas)
                    VALUES (@IdPedido, @EstadoAnterior, 'Entregado', GETDATE(), @CambiadoPor, @Notas)", conn);
                hist.Parameters.Add(SqlParameterHelper.Int("@IdPedido", idPedido));
                hist.Parameters.Add(SqlParameterHelper.String("@EstadoAnterior", 50, estadoActual));
                hist.Parameters.Add(SqlParameterHelper.String("@CambiadoPor", 100, $"Usuario_{idUsuario}"));
                hist.Parameters.Add(SqlParameterHelper.String("@Notas", 500, "Cliente confirmó recepción del pedido"));
                await hist.ExecuteNonQueryAsync();
            }
            catch (SqlException ex) when (ex.Number == 208) { }

            return true;
        }

        public async Task<int> CountAsync()
        {
            return await WithConnectionAsync(async conn =>
            {
                using var cmd = new SqlCommand("SELECT COUNT(*) FROM Pedidos", conn);
                var result = await cmd.ExecuteScalarAsync();
                return result != null ? (int)result : 0;
            });
        }
    }
}
