using Microsoft.AspNetCore.Mvc;
using EnerGym.Services.Interfaces;

namespace EnerGym.Controllers
{
    [ApiController]
    [Route("api/mensajes")]
    public class MensajesController : BaseApiController
    {
        private readonly IMensajeService _mensajeService;

        public MensajesController(IMensajeService mensajeService)
        {
            _mensajeService = mensajeService;
        }

        [HttpPost]
        public async Task<IActionResult> CrearMensaje([FromBody] Repositories.Interfaces.CrearMensajeDto dto)
        {
            var id = await _mensajeService.CrearAsync(dto);
            return OkData(new { message = "Mensaje enviado correctamente", idMensaje = id });
        }

        [HttpGet]
        public async Task<IActionResult> GetMensajes()
        {
            var mensajes = await _mensajeService.GetAllAsync();
            return OkData(mensajes);
        }

        [HttpGet("usuario/{idUsuario:int}")]
        public async Task<IActionResult> GetMensajesUsuario(int idUsuario)
        {
            var mensajes = await _mensajeService.GetByUsuarioAsync(idUsuario);
            return OkData(mensajes);
        }

        [HttpPut("{idMensaje:int}/responder")]
        public async Task<IActionResult> ResponderMensaje(int idMensaje, [FromBody] Models.ResponderMensajeDto dto)
        {
            var updated = await _mensajeService.ResponderAsync(idMensaje, dto.Respuesta);
            if (!updated) return NotFoundResponse("Mensaje no encontrado.");
            return OkMessage("Respuesta guardada correctamente.");
        }

        [HttpPut("{idMensaje:int}/leido")]
        public async Task<IActionResult> MarcarLeido(int idMensaje)
        {
            var updated = await _mensajeService.MarcarLeidoAsync(idMensaje);
            if (!updated) return NotFoundResponse("Mensaje no encontrado.");
            return OkMessage("Mensaje marcado como leído.");
        }

        [HttpPut("{idMensaje:int}/respondido")]
        public async Task<IActionResult> MarcarRespondido(int idMensaje)
        {
            var updated = await _mensajeService.MarcarRespondidoAsync(idMensaje);
            if (!updated) return NotFoundResponse("Mensaje no encontrado.");
            return OkMessage("Mensaje marcado como respondido.");
        }

        [HttpDelete("{idMensaje:int}")]
        public async Task<IActionResult> EliminarMensaje(int idMensaje)
        {
            var deleted = await _mensajeService.EliminarAsync(idMensaje);
            if (!deleted) return NotFoundResponse("Mensaje no encontrado.");
            return OkMessage("Mensaje eliminado.");
        }

        [HttpGet("no-leidos/count")]
        public async Task<IActionResult> GetCountNoLeidos()
        {
            var count = await _mensajeService.CountNoLeidosAsync();
            return OkData(new { count });
        }
    }
}
