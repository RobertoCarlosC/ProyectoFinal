using EnerGym.Repositories.Interfaces;

namespace EnerGym.Services.Interfaces
{
    public interface ILikeService
    {
        Task<bool> ToggleAsync(int idUsuario, int idProducto);
        Task<int> CountByProductoAsync(int idProducto);
        Task<List<ProductoLikeDto>> GetByUsuarioAsync(int idUsuario);
    }
}
