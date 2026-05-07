using EnerGym.Models;
using EnerGym.Repositories.Interfaces;

namespace EnerGym.Services.Interfaces
{
    public interface ICarritoService
    {
        Task<CarritoDto?> GetCarritoAsync(int idUsuario);
        Task<bool> AddItemAsync(AddCarritoDto dto);
        Task<bool> UpdateCantidadAsync(int idItem, int cantidad);
        Task<bool> RemoveItemAsync(int idItem);
    }
}
