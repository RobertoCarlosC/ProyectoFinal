using EnerGym.Models;

namespace EnerGym.Repositories.Interfaces
{
    public interface ICarritoRepository
    {
        Task<CarritoDto?> GetCarritoAsync(int idUsuario);
        Task<bool> AddItemAsync(AddCarritoDto dto);
        Task<bool> UpdateCantidadAsync(int idItem, int cantidad);
        Task<bool> RemoveItemAsync(int idItem);
        Task<int?> GetStockAsync(int idProducto);
        Task<int> GetCarritoIdAsync(int idUsuario);
        Task<bool> ClearCarritoAsync(int idCarrito);
        Task<bool> ClearCarritoByUsuarioAsync(int idUsuario);
    }

    public class CarritoDto
    {
        public int IdCarrito { get; set; }
        public List<CarritoItemDto> Items { get; set; } = new();
        public decimal Total { get; set; }
    }

    public class CarritoItemDto
    {
        public int Id { get; set; }
        public int IdCarrito { get; set; }
        public int IdProducto { get; set; }
        public string NombreProducto { get; set; } = "";
        public string Imagen { get; set; } = "";
        public decimal Precio { get; set; }
        public int Cantidad { get; set; }
        public decimal Subtotal { get; set; }
    }
}
