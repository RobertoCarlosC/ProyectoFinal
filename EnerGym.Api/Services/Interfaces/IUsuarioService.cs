using EnerGym.Models;
using EnerGym.Repositories.Interfaces;

namespace EnerGym.Services.Interfaces
{
    public interface IUsuarioService
    {
        Task<PerfilDto?> GetPerfilAsync(int id);
        Task<bool> UpdatePerfilAsync(EditarPerfilDto dto);
        Task<bool> ChangePasswordAsync(CambiarContraseñaDto dto);
        Task<List<UsuarioListDto>> GetAllAsync();
        Task<bool> IsAdminAsync(int id);
    }
}
