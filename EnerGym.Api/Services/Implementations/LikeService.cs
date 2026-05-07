using EnerGym.Repositories.Interfaces;
using EnerGym.Services.Interfaces;

namespace EnerGym.Services.Implementations
{
    public class LikeService : ILikeService
    {
        private readonly ILikeRepository _repo;

        public LikeService(ILikeRepository repo)
        {
            _repo = repo;
        }

        public async Task<bool> ToggleAsync(int idUsuario, int idProducto)
        {
            return await _repo.ToggleAsync(idUsuario, idProducto);
        }

        public async Task<int> CountByProductoAsync(int idProducto)
        {
            return await _repo.CountByProductoAsync(idProducto);
        }

        public async Task<List<ProductoLikeDto>> GetByUsuarioAsync(int idUsuario)
        {
            return await _repo.GetByUsuarioAsync(idUsuario);
        }
    }
}
