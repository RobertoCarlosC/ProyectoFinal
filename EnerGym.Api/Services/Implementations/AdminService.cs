using EnerGym.Repositories.Interfaces;
using EnerGym.Services.Interfaces;

namespace EnerGym.Services.Implementations
{
    public class AdminService : IAdminService
    {
        private readonly IUsuarioRepository _usuarioRepo;
        private readonly IPedidoRepository _pedidoRepo;
        private readonly IProductoRepository _productoRepo;

        public AdminService(IUsuarioRepository usuarioRepo, IPedidoRepository pedidoRepo, IProductoRepository productoRepo)
        {
            _usuarioRepo = usuarioRepo;
            _pedidoRepo = pedidoRepo;
            _productoRepo = productoRepo;
        }

        public async Task<bool> IsAdminAsync(int id)
        {
            return await _usuarioRepo.IsAdminAsync(id);
        }

        public async Task<AdminDashboardDto> GetDashboardAsync()
        {
            var totalUsuarios = await _usuarioRepo.CountClientesAsync();
            var totalPedidos = await _pedidoRepo.CountAsync();
            var totalProductos = await _productoRepo.CountAsync();
            var sinStock = await _productoRepo.CountSinStockAsync();
            var stats = await _pedidoRepo.GetEstadisticasAsync();

            return new AdminDashboardDto
            {
                TotalUsuarios = totalUsuarios,
                TotalPedidos = totalPedidos,
                IngresosTotales = stats.VentasTotal,
                TotalProductos = totalProductos,
                ProductosSinStock = sinStock,
                PedidosPorEstado = new List<EstadoDistribucionDto>() // Se llena desde controller si es necesario
            };
        }
    }
}
