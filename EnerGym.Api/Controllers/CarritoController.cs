using Microsoft.AspNetCore.Mvc;
using EnerGym.Models;
using EnerGym.Services.Interfaces;

namespace EnerGym.Controllers
{
    [ApiController]
    [Route("api/carrito")]
    public class CarritoController : BaseApiController
    {
        private readonly ICarritoService _carritoService;

        public CarritoController(ICarritoService carritoService)
        {
            _carritoService = carritoService;
        }

        [HttpGet("{idUsuario:int}")]
        public async Task<IActionResult> GetCarrito(int idUsuario)
        {
            var carrito = await _carritoService.GetCarritoAsync(idUsuario);
            if (carrito == null) return NotFoundResponse("Carrito no encontrado.");
            return OkData(new { idCarrito = carrito.IdCarrito, items = carrito.Items, total = carrito.Total });
        }

        [HttpPost("agregar")]
        public async Task<IActionResult> AgregarProducto([FromBody] AddCarritoDto dto)
        {
            if (dto.Cantidad <= 0)
                return BadRequestResponse("La cantidad debe ser mayor que 0.");

            var result = await _carritoService.AddItemAsync(dto);
            if (!result) return BadRequestResponse("No se pudo añadir el producto. Verifica el stock.");
            return OkMessage("Producto añadido al carrito.");
        }

        [HttpPut("item/{id:int}")]
        public async Task<IActionResult> UpdateCantidad(int id, [FromBody] UpdateCantidadDto dto)
        {
            var result = await _carritoService.UpdateCantidadAsync(id, dto.Cantidad);
            if (!result) return NotFoundResponse("Item no encontrado o stock insuficiente.");
            return OkMessage("Cantidad actualizada.");
        }

        [HttpDelete("item/{id:int}")]
        public async Task<IActionResult> EliminarItem(int id)
        {
            var result = await _carritoService.RemoveItemAsync(id);
            if (!result) return NotFoundResponse("Item no encontrado.");
            return OkMessage("Producto eliminado del carrito.");
        }
    }
}
