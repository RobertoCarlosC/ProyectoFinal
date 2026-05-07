using EnerGym.Models;
using EnerGym.Repositories.Interfaces;
using EnerGym.Services.Interfaces;

namespace EnerGym.Services.Implementations
{
    public class CarritoService : ICarritoService
    {
        private readonly ICarritoRepository _repo;

        public CarritoService(ICarritoRepository repo)
        {
            _repo = repo;
        }

        public async Task<CarritoDto?> GetCarritoAsync(int idUsuario)
        {
            return await _repo.GetCarritoAsync(idUsuario);
        }

        public async Task<bool> AddItemAsync(AddCarritoDto dto)
        {
            if (dto.Cantidad <= 0)
                throw new InvalidOperationException("La cantidad debe ser mayor que 0.");

            return await _repo.AddItemAsync(dto);
        }

        public async Task<bool> UpdateCantidadAsync(int idItem, int cantidad)
        {
            return await _repo.UpdateCantidadAsync(idItem, cantidad);
        }

        public async Task<bool> RemoveItemAsync(int idItem)
        {
            return await _repo.RemoveItemAsync(idItem);
        }
    }
}
