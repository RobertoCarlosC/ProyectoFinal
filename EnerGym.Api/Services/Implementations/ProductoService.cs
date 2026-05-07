using EnerGym.Repositories.Interfaces;
using EnerGym.Services.Interfaces;

namespace EnerGym.Services.Implementations
{
    public class ProductoService : IProductoService
    {
        private readonly IProductoRepository _repo;

        public ProductoService(IProductoRepository repo)
        {
            _repo = repo;
        }

        public async Task<List<ProductoListDto>> GetAllAsync(int? idUsuario = null)
        {
            return await _repo.GetAllAsync(idUsuario);
        }

        public async Task<List<CategoriaDto>> GetCategoriasAsync()
        {
            return await _repo.GetCategoriasAsync();
        }

        public async Task<ProductoDetailDto?> GetByIdAsync(int id, int? idUsuario = null)
        {
            return await _repo.GetByIdAsync(id, idUsuario);
        }

        public async Task<int> CreateAsync(EnerGym.Models.ProductoDto dto)
        {
            return await _repo.CreateAsync(dto);
        }

        public async Task<bool> UpdateAsync(int id, EnerGym.Models.ProductoDto dto)
        {
            return await _repo.UpdateAsync(id, dto);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await _repo.DeleteAsync(id);
        }
    }
}
