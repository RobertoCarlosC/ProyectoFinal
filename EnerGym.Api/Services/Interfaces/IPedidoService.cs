using EnerGym.Models;
using EnerGym.Repositories.Interfaces;

namespace EnerGym.Services.Interfaces
{
    public interface IPedidoService
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
    }
}
