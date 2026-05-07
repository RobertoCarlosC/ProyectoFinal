using EnerGym.Models;
using EnerGym.Repositories.Interfaces;
using EnerGym.Services.Interfaces;

namespace EnerGym.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly IAuthRepository _repo;

        public AuthService(IAuthRepository repo)
        {
            _repo = repo;
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _repo.EmailExistsAsync(email);
        }

        public async Task RegisterAsync(RegisterDto dto)
        {
            if (await _repo.EmailExistsAsync(dto.Email.Trim().ToLower()))
                throw new InvalidOperationException("El email ya está registrado.");

            await _repo.RegisterAsync(dto);
        }

        public async Task<LoginResponse?> LoginAsync(string email, string password)
        {
            var user = await _repo.LoginAsync(email.Trim().ToLower());
            if (user == null) return null;

            bool valid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
            if (!valid) return null;

            return new LoginResponse
            {
                IdUsuario = user.IdUsuario,
                Nombre = user.Nombre,
                Email = user.Email,
                IdRol = user.IdRol
            };
        }
    }
}
