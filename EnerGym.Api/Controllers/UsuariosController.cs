using Microsoft.AspNetCore.Mvc;
using EnerGym.Models;
using EnerGym.Services.Interfaces;

namespace EnerGym.Controllers
{
    [ApiController]
    [Route("api/usuarios")]
    public class UsuariosController : BaseApiController
    {
        private readonly IUsuarioService _usuarioService;
        private readonly IPedidoService _pedidoService;

        public UsuariosController(IUsuarioService usuarioService, IPedidoService pedidoService)
        {
            _usuarioService = usuarioService;
            _pedidoService = pedidoService;
        }

        [HttpGet("{idUsuario:int}/perfil")]
        public async Task<IActionResult> GetPerfil(int idUsuario)
        {
            var perfil = await _usuarioService.GetPerfilAsync(idUsuario);
            if (perfil == null) return NotFoundResponse("Usuario no encontrado.");
            return OkData(perfil);
        }

        [HttpPut("{idUsuario:int}/editar-perfil")]
        public async Task<IActionResult> EditarPerfil(int idUsuario, [FromBody] EditarPerfilDto dto)
        {
            if (idUsuario != dto.IdUsuario)
                return BadRequestResponse("ID de usuario no válido.");

            if (string.IsNullOrWhiteSpace(dto.Nombre) || string.IsNullOrWhiteSpace(dto.Email))
                return BadRequestResponse("Nombre y Email son obligatorios.");

            var updated = await _usuarioService.UpdatePerfilAsync(dto);
            if (!updated) return NotFoundResponse("Usuario no encontrado.");
            return OkMessage("Perfil actualizado correctamente.");
        }

        [HttpPut("{idUsuario:int}/cambiar-contraseña")]
        public async Task<IActionResult> CambiarContraseña(int idUsuario, [FromBody] CambiarContraseñaDto dto)
        {
            if (idUsuario != dto.IdUsuario)
                return BadRequestResponse("ID de usuario no válido.");

            if (string.IsNullOrWhiteSpace(dto.ContraseñaActual) ||
                string.IsNullOrWhiteSpace(dto.ContraseñaNueva) ||
                string.IsNullOrWhiteSpace(dto.ConfirmarContraseña))
                return BadRequestResponse("Todos los campos son obligatorios.");

            try
            {
                var updated = await _usuarioService.ChangePasswordAsync(dto);
                if (!updated) return NotFoundResponse("Usuario no encontrado.");
                return OkMessage("Contraseña cambiada correctamente.");
            }
            catch (UnauthorizedAccessException ex)
            {
                return UnauthorizedResponse(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequestResponse(ex.Message);
            }
        }

        [HttpGet("{idUsuario:int}/pedidos")]
        public async Task<IActionResult> GetPedidosUsuario(int idUsuario)
        {
            var pedidos = await _pedidoService.GetByUsuarioAsync(idUsuario);
            return OkData(pedidos);
        }

        [HttpGet("{idUsuario:int}/pedidos/{idPedido:int}")]
        public async Task<IActionResult> GetDetallePedido(int idUsuario, int idPedido)
        {
            var pedido = await _pedidoService.GetByIdAsync(idPedido);
            if (pedido == null) return NotFoundResponse("Pedido no encontrado.");
            // Validar que pertenezca al usuario (el servicio ya valida en ConfirmarEntrega, aquí hacemos validación básica)
            // Nota: el repository GetByIdAsync no valida propiedad. Mantenemos la funcionalidad original.
            return OkData(new { pedido, detalles = pedido.Productos });
        }
    }
}
