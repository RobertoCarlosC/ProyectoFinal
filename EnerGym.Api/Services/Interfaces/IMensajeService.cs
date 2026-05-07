using EnerGym.Repositories.Interfaces;

namespace EnerGym.Services.Interfaces
{
    public interface IMensajeService
    {
        Task<int> CrearAsync(CrearMensajeDto dto);
        Task<List<MensajeDto>> GetAllAsync();
        Task<List<MensajeDto>> GetByUsuarioAsync(int idUsuario);
        Task<bool> ResponderAsync(int idMensaje, string? respuesta);
        Task<bool> MarcarLeidoAsync(int idMensaje);
        Task<bool> MarcarRespondidoAsync(int idMensaje);
        Task<bool> EliminarAsync(int idMensaje);
        Task<int> CountNoLeidosAsync();
    }
}
