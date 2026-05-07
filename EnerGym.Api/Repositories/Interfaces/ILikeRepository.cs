namespace EnerGym.Repositories.Interfaces
{
    public interface ILikeRepository
    {
        Task<bool> ToggleAsync(int idUsuario, int idProducto);
        Task<int> CountByProductoAsync(int idProducto);
        Task<List<ProductoLikeDto>> GetByUsuarioAsync(int idUsuario);
    }

    public class ProductoLikeDto
    {
        public int IdProducto { get; set; }
        public string Nombre { get; set; } = "";
        public decimal Precio { get; set; }
        public int Stock { get; set; }
        public string Imagen { get; set; } = "";
    }
}
