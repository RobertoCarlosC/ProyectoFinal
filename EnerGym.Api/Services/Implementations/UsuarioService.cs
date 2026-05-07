using EnerGym.Models;
using EnerGym.Repositories.Interfaces;
using EnerGym.Services.Interfaces;

namespace EnerGym.Services.Implementations
{
    public class UsuarioService : IUsuarioService
    {
        private readonly IUsuarioRepository _repo;

        public UsuarioService(IUsuarioRepository repo)
        {
            _repo = repo;
        }

        public async Task<PerfilDto?> GetPerfilAsync(int id)
        {
            return await _repo.GetPerfilAsync(id);
        }

        public async Task<bool> UpdatePerfilAsync(EditarPerfilDto dto)
        {
            if (await _repo.EmailExistsAsync(dto.Email, dto.IdUsuario))
                throw new InvalidOperationException("El email ya está registrado a otro usuario.");

            return await _repo.UpdatePerfilAsync(dto);
        }

        public async Task<bool> ChangePasswordAsync(CambiarContraseñaDto dto)
        {
            if (dto.ContraseñaNueva != dto.ConfirmarContraseña)
                throw new InvalidOperationException("Las contraseñas no coinciden.");

            if (dto.ContraseñaNueva.Length < 6)
                throw new InvalidOperationException("La contraseña debe tener al menos 6 caracteres.");

            var hashActual = await _repo.GetPasswordHashAsync(dto.IdUsuario);
            if (hashActual == null)
                throw new InvalidOperationException("Usuario no encontrado.");

            bool valid = BCrypt.Net.BCrypt.Verify(dto.ContraseñaActual, hashActual);
            if (!valid)
                throw new UnauthorizedAccessException("Contraseña actual incorrecta.");

            string nuevoHash = BCrypt.Net.BCrypt.HashPassword(dto.ContraseñaNueva);
            return await _repo.UpdatePasswordAsync(dto.IdUsuario, nuevoHash);
        }

        public async Task<List<UsuarioListDto>> GetAllAsync()
        {
            return await _repo.GetAllAsync();
        }

        public async Task<bool> IsAdminAsync(int id)
        {
            return await _repo.IsAdminAsync(id);
        }
    }
}
