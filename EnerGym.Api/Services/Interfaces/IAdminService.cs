namespace EnerGym.Services.Interfaces
{
    public interface IAdminService
    {
        Task<bool> IsAdminAsync(int id);
        Task<AdminDashboardDto> GetDashboardAsync();
    }

    public class AdminDashboardDto
    {
        public int TotalUsuarios { get; set; }
        public int TotalPedidos { get; set; }
        public decimal IngresosTotales { get; set; }
        public int TotalProductos { get; set; }
        public int ProductosSinStock { get; set; }
        public List<EstadoDistribucionDto> PedidosPorEstado { get; set; } = new();
    }

    public class EstadoDistribucionDto
    {
        public string Estado { get; set; } = "";
        public int Cantidad { get; set; }
    }
}
