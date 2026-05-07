using EnerGym.Repositories.Interfaces;
using EnerGym.Services.Interfaces;

namespace EnerGym.Services.Implementations
{
    public class MensajeService : IMensajeService
    {
        private readonly IMensajeRepository _repo;

        public MensajeService(IMensajeRepository repo)
        {
            _repo = repo;
        }

        public async Task<int> CrearAsync(CrearMensajeDto dto)
        {
            return await _repo.CrearAsync(dto);
        }

        public async Task<List<MensajeDto>> GetAllAsync()
        {
            return await _repo.GetAllAsync();
        }

        public async Task<List<MensajeDto>> GetByUsuarioAsync(int idUsuario)
        {
            return await _repo.GetByUsuarioAsync(idUsuario);
        }

        public async Task<bool> ResponderAsync(int idMensaje, string? respuesta)
        {
            return await _repo.ResponderAsync(idMensaje, respuesta);
        }

        public async Task<bool> MarcarLeidoAsync(int idMensaje)
        {
            return await _repo.MarcarLeidoAsync(idMensaje);
        }

        public async Task<bool> MarcarRespondidoAsync(int idMensaje)
        {
            return await _repo.MarcarRespondidoAsync(idMensaje);
        }

        public async Task<bool> EliminarAsync(int idMensaje)
        {
            return await _repo.EliminarAsync(idMensaje);
        }

        public async Task<int> CountNoLeidosAsync()
        {
            return await _repo.CountNoLeidosAsync();
        }
    }
}
