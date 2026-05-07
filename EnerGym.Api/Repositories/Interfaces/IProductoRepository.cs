using EnerGym.Models;

namespace EnerGym.Repositories.Interfaces
{
    public interface IProductoRepository
    {
        Task<List<ProductoListDto>> GetAllAsync(int? idUsuario = null);
        Task<List<CategoriaDto>> GetCategoriasAsync();
        Task<ProductoDetailDto?> GetByIdAsync(int id, int? idUsuario = null);
        Task<int> CreateAsync(ProductoDto dto);
        Task<bool> UpdateAsync(int id, ProductoDto dto);
        Task<bool> DeleteAsync(int id);
        Task<int> CountAsync();
        Task<int> CountSinStockAsync();
    }

    public class ProductoListDto
    {
        public int IdProducto { get; set; }
        public string Nombre { get; set; } = "";
        public string Descripcion { get; set; } = "";
        public decimal Precio { get; set; }
        public int Stock { get; set; }
        public string Imagen { get; set; } = "";
        public int IdCategoria { get; set; }
        public string NombreCategoria { get; set; } = "";
        public bool TieneLike { get; set; }
        public int TotalLikes { get; set; }
    }

    public class ProductoDetailDto : ProductoListDto
    {
        public List<ProductoImagenDto> Imagenes { get; set; } = new();
    }

    public class ProductoImagenDto
    {
        public int IdImagen { get; set; }
        public string UrlImagen { get; set; } = "";
        public int Orden { get; set; }
    }

    public class CategoriaDto
    {
        public int IdCategoria { get; set; }
        public string Nombre { get; set; } = "";
        public string Descripcion { get; set; } = "";
    }
}
