using Microsoft.AspNetCore.Mvc;
using EnerGym.Api.Models;

namespace EnerGym.Api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private static List<User> users = new()
        {
            new User { Id = 1, Email = "admin@energym.com", Password = "1234", TipoUsuario = "admin" }
        };

        [HttpPost("login")]
        public IActionResult Login(User login)
        {
            var user = users.FirstOrDefault(u =>
                u.Email == login.Email && u.Password == login.Password);

            if (user == null)
                return Unauthorized("Credenciales incorrectas");

            return Ok(user);
        }
    }
}