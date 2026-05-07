using EnerGym.Models;

namespace EnerGym.Repositories.Interfaces
{
    public interface IAuthRepository
    {
        Task<bool> EmailExistsAsync(string email);
        Task RegisterAsync(RegisterDto dto);
        Task<LoginResponse?> LoginAsync(string email);
    }
}
