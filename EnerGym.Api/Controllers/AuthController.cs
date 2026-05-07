using Microsoft.AspNetCore.Mvc;
using EnerGym.Models;
using EnerGym.Services.Interfaces;

namespace EnerGym.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : BaseApiController
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Nombre) ||
                string.IsNullOrWhiteSpace(dto.Email) ||
                string.IsNullOrWhiteSpace(dto.Password))
            {
                return BadRequestResponse("Todos los campos son obligatorios.");
            }

            if (await _authService.EmailExistsAsync(dto.Email))
                return ConflictResponse("El email ya está registrado.");

            await _authService.RegisterAsync(dto);
            return OkMessage("Usuario registrado correctamente.");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
                return BadRequestResponse("Email y contraseña son obligatorios.");

            var user = await _authService.LoginAsync(dto.Email, dto.Password);
            if (user == null)
                return UnauthorizedResponse("Email o contraseña incorrectos.");

            return OkData(new
            {
                user.IdUsuario,
                user.Nombre,
                user.Email,
                user.IdRol
            });
        }
    }
}
