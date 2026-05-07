using EnerGym.Models;

namespace EnerGym.Repositories.Interfaces
{
    public interface IUsuarioRepository
    {
        Task<Usuario?> GetByIdAsync(int id);
        Task<PerfilDto?> GetPerfilAsync(int id);
        Task<bool> UpdatePerfilAsync(EditarPerfilDto dto);
        Task<bool> UpdatePasswordAsync(int idUsuario, string newHash);
        Task<string?> GetPasswordHashAsync(int idUsuario);
        Task<bool> EmailExistsAsync(string email, int? excludeId = null);
        Task<List<UsuarioListDto>> GetAllAsync();
        Task<bool> UpdateAsync(EditarUsuarioAdminDto dto);
        Task<bool> DeleteAsync(int id);
        Task<bool> IsAdminAsync(int id);
        Task<int> CountClientesAsync();
    }

    public class PerfilDto
    {
        public int IdUsuario { get; set; }
        public string Nombre { get; set; } = "";
        public string Email { get; set; } = "";
        public string Telefono { get; set; } = "";
        public string Direccion { get; set; } = "";
        public string Ciudad { get; set; } = "";
        public string CodigoPostal { get; set; } = "";
        public string FotoPerfil { get; set; } = "";
        public DateTime FechaRegistro { get; set; }
    }

    public class UsuarioListDto
    {
        public int IdUsuario { get; set; }
        public string Nombre { get; set; } = "";
        public string Email { get; set; } = "";
        public DateTime FechaRegistro { get; set; }
        public string Rol { get; set; } = "";
        public int IdRol { get; set; }
    }
}
