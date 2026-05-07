using EnerGym.Models;

namespace EnerGym.Services.Interfaces
{
    public interface IAuthService
    {
        Task<bool> EmailExistsAsync(string email);
        Task RegisterAsync(RegisterDto dto);
        Task<LoginResponse?> LoginAsync(string email, string password);
    }
}
