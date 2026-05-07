using Microsoft.AspNetCore.Mvc;
using EnerGym.Models;
using EnerGym.Services.Interfaces;

namespace EnerGym.Controllers
{
    [ApiController]
    [Route("api/pedidos")]
    public class PedidosController : BaseApiController
    {
        private readonly IPedidoService _pedidoService;

        public PedidosController(IPedidoService pedidoService)
        {
            _pedidoService = pedidoService;
        }

        [HttpPost("confirmar")]
        public async Task<IActionResult> ConfirmarPedido([FromBody] ConfirmarPedidoDto dto)
        {
            var resultado = await _pedidoService.ConfirmarAsync(dto);
            if (resultado == null)
                return BadRequestResponse("No se pudo confirmar el pedido. Verifica que tengas productos en el carrito y stock disponible.");

            return OkData(new { message = "Pedido confirmado correctamente.", idPedido = resultado.IdPedido, total = resultado.Total });
        }

        [HttpGet("detalle/{idPedido:int}")]
        public async Task<IActionResult> GetDetallePedido(int idPedido)
        {
            var pedido = await _pedidoService.GetByIdAsync(idPedido);
            if (pedido == null) return NotFoundResponse("Pedido no encontrado.");
            return OkData(new { pedido, productos = pedido.Productos });
        }

        [HttpGet("{idUsuario:int}")]
        public async Task<IActionResult> GetPedidos(int idUsuario)
        {
            var pedidos = await _pedidoService.GetByUsuarioAsync(idUsuario);
            return OkData(pedidos);
        }

        [HttpGet("admin/todos")]
        public async Task<IActionResult> GetAllPedidos([FromQuery] string? estado = null, [FromQuery] int? idUsuario = null)
        {
            var pedidos = await _pedidoService.GetAllAsync(estado, idUsuario);
            return OkData(pedidos);
        }

        [HttpGet("admin/estadisticas")]
        public async Task<IActionResult> GetEstadisticas()
        {
            var stats = await _pedidoService.GetEstadisticasAsync();
            return OkData(stats);
        }

        [HttpGet("admin/ventas-por-dia")]
        public async Task<IActionResult> GetVentasPorDia([FromQuery] int dias = 14)
        {
            var ventas = await _pedidoService.GetVentasPorDiaAsync(dias);
            return OkData(ventas);
        }

        [HttpPost("admin/{idPedido:int}/estado")]
        public async Task<IActionResult> CambiarEstadoAdmin(int idPedido, [FromBody] CambiarEstadoPedidoAdminDto dto)
        {
            if (idPedido != dto.IdPedido)
                return BadRequestResponse("El ID del pedido no coincide.");

            try
            {
                var updated = await _pedidoService.CambiarEstadoAsync(idPedido, dto.NuevoEstado, dto.IdAdmin, dto.Notas);
                if (!updated) return NotFoundResponse("Pedido no encontrado.");
                return OkData(new { message = "Estado actualizado correctamente.", idPedido, nuevoEstado = dto.NuevoEstado });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequestResponse(ex.Message);
            }
        }

        [HttpGet("{idPedido:int}/historial")]
        public async Task<IActionResult> GetHistorialPedido(int idPedido)
        {
            var historial = await _pedidoService.GetHistorialAsync(idPedido);
            return OkData(historial);
        }

        [HttpPost("{idPedido:int}/confirmar-entrega")]
        public async Task<IActionResult> ConfirmarEntrega(int idPedido, [FromBody] ConfirmarEntregaDto dto)
        {
            if (idPedido != dto.IdPedido)
                return BadRequestResponse("El ID del pedido no coincide.");

            var result = await _pedidoService.ConfirmarEntregaAsync(idPedido, dto.IdUsuario);
            if (!result) return BadRequestResponse("No se pudo confirmar la entrega.");
            return OkData(new { message = "Entrega confirmada correctamente.", idPedido });
        }
    }
}
