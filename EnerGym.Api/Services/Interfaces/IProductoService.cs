using EnerGym.Repositories.Interfaces;

namespace EnerGym.Services.Interfaces
{
    public interface IProductoService
    {
        Task<List<ProductoListDto>> GetAllAsync(int? idUsuario = null);
        Task<List<CategoriaDto>> GetCategoriasAsync();
        Task<ProductoDetailDto?> GetByIdAsync(int id, int? idUsuario = null);
        Task<int> CreateAsync(EnerGym.Models.ProductoDto dto);
        Task<bool> UpdateAsync(int id, EnerGym.Models.ProductoDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
