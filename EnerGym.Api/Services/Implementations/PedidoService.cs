using EnerGym.Models;
using EnerGym.Repositories.Interfaces;
using EnerGym.Services.Interfaces;

namespace EnerGym.Services.Implementations
{
    public class PedidoService : IPedidoService
    {
        private readonly IPedidoRepository _repo;

        public PedidoService(IPedidoRepository repo)
        {
            _repo = repo;
        }

        public async Task<PedidoConfirmadoDto?> ConfirmarAsync(ConfirmarPedidoDto dto)
        {
            return await _repo.ConfirmarAsync(dto);
        }

        public async Task<PedidoDetalleDto?> GetByIdAsync(int idPedido)
        {
            return await _repo.GetByIdAsync(idPedido);
        }

        public async Task<List<PedidoResumenDto>> GetByUsuarioAsync(int idUsuario)
        {
            return await _repo.GetByUsuarioAsync(idUsuario);
        }

        public async Task<List<PedidoAdminDto>> GetAllAsync(string? estado = null, int? idUsuario = null)
        {
            return await _repo.GetAllAsync(estado, idUsuario);
        }

        public async Task<EstadisticasDto> GetEstadisticasAsync()
        {
            return await _repo.GetEstadisticasAsync();
        }

        public async Task<List<VentaDiaDto>> GetVentasPorDiaAsync(int dias)
        {
            return await _repo.GetVentasPorDiaAsync(dias);
        }

        public async Task<bool> CambiarEstadoAsync(int idPedido, string nuevoEstado, int idAdmin, string? notas)
        {
            var estadosValidos = new[] { "Pendiente", "Procesando", "Enviado", "En reparto", "Entregado" };
            if (!estadosValidos.Contains(nuevoEstado))
                throw new InvalidOperationException("Estado inválido.");

            return await _repo.CambiarEstadoAsync(idPedido, nuevoEstado, idAdmin, notas);
        }

        public async Task<List<HistorialEstadoDto>> GetHistorialAsync(int idPedido)
        {
            return await _repo.GetHistorialAsync(idPedido);
        }

        public async Task<bool> ConfirmarEntregaAsync(int idPedido, int idUsuario)
        {
            return await _repo.ConfirmarEntregaAsync(idPedido, idUsuario);
        }
    }
}
