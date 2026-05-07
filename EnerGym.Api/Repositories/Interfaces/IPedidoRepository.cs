using EnerGym.Models;

namespace EnerGym.Repositories.Interfaces
{
    public interface IPedidoRepository
    {
        Task<PedidoConfirmadoDto?> ConfirmarAsync(ConfirmarPedidoDto dto);
        Task<PedidoDetalleDto?> GetByIdAsync(int idPedido);
        Task<List<PedidoResumenDto>> GetByUsuarioAsync(int idUsuario);
        Task<List<PedidoAdminDto>> GetAllAsync(string? estado = null, int? idUsuario = null);
        Task<EstadisticasDto> GetEstadisticasAsync();
        Task<List<VentaDiaDto>> GetVentasPorDiaAsync(int dias);
        Task<bool> CambiarEstadoAsync(int idPedido, string nuevoEstado, int idAdmin, string? notas);
        Task<List<HistorialEstadoDto>> GetHistorialAsync(int idPedido);
        Task<bool> ConfirmarEntregaAsync(int idPedido, int idUsuario);
        Task<int> CountAsync();
    }

    public class PedidoConfirmadoDto
    {
        public int IdPedido { get; set; }
        public decimal Total { get; set; }
    }

    public class PedidoDetalleDto
    {
        public int IdPedido { get; set; }
        public DateTime Fecha { get; set; }
        public decimal Total { get; set; }
        public string Estado { get; set; } = "";
        public string? DireccionEnvio { get; set; }
        public string? MetodoPago { get; set; }
        public List<PedidoProductoDto> Productos { get; set; } = new();
    }

    public class PedidoProductoDto
    {
        public int IdDetalle { get; set; }
        public int IdProducto { get; set; }
        public string NombreProducto { get; set; } = "";
        public string Imagen { get; set; } = "";
        public int Cantidad { get; set; }
        public decimal Precio { get; set; }
        public decimal Subtotal { get; set; }
    }

    public class PedidoResumenDto
    {
        public int IdPedido { get; set; }
        public DateTime Fecha { get; set; }
        public decimal Total { get; set; }
        public string Estado { get; set; } = "";
    }

    public class PedidoAdminDto
    {
        public int IdPedido { get; set; }
        public int IdUsuario { get; set; }
        public string UsuarioNombre { get; set; } = "";
        public string UsuarioEmail { get; set; } = "";
        public DateTime Fecha { get; set; }
        public decimal Total { get; set; }
        public string Estado { get; set; } = "";
        public int CantidadProductos { get; set; }
    }

    public class EstadisticasDto
    {
        public int TotalPedidos { get; set; }
        public int Pendientes { get; set; }
        public int EnProceso { get; set; }
        public int Enviados { get; set; }
        public int Entregados { get; set; }
        public decimal VentasTotal { get; set; }
        public decimal PromedioVenta { get; set; }
    }

    public class VentaDiaDto
    {
        public string Fecha { get; set; } = "";
        public int TotalPedidos { get; set; }
        public decimal Ventas { get; set; }
    }

    public class HistorialEstadoDto
    {
        public int IdHistorial { get; set; }
        public int IdPedido { get; set; }
        public string? EstadoAnterior { get; set; }
        public string EstadoNuevo { get; set; } = "";
        public DateTime Fecha { get; set; }
        public string? CambiadoPor { get; set; }
        public string? Notas { get; set; }
    }
}
