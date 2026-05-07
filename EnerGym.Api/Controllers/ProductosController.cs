using Microsoft.AspNetCore.Mvc;
using EnerGym.Models;
using EnerGym.Services.Interfaces;

namespace EnerGym.Controllers
{
    [ApiController]
    [Route("api/productos")]
    public class ProductosController : BaseApiController
    {
        private readonly IProductoService _productoService;

        public ProductosController(IProductoService productoService)
        {
            _productoService = productoService;
        }

        [HttpGet]
        public async Task<IActionResult> GetProductos([FromQuery] int? idUsuario)
        {
            var productos = await _productoService.GetAllAsync(idUsuario);
            return OkData(productos);
        }

        [HttpGet("categorias")]
        public async Task<IActionResult> GetCategorias()
        {
            var categorias = await _productoService.GetCategoriasAsync();
            return OkData(categorias);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetProducto(int id, [FromQuery] int? idUsuario)
        {
            var producto = await _productoService.GetByIdAsync(id, idUsuario);
            if (producto == null) return NotFoundResponse("Producto no encontrado.");
            return OkData(new
            {
                producto.IdProducto,
                producto.Nombre,
                producto.Descripcion,
                producto.Precio,
                producto.Stock,
                producto.Imagen,
                producto.IdCategoria,
                producto.NombreCategoria,
                producto.TieneLike,
                producto.TotalLikes,
                producto.Imagenes
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateProducto([FromBody] ProductoDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Nombre))
                return BadRequestResponse("El nombre es obligatorio.");

            var id = await _productoService.CreateAsync(dto);
            return OkMessage("Producto creado correctamente.");
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateProducto(int id, [FromBody] ProductoDto dto)
        {
            var updated = await _productoService.UpdateAsync(id, dto);
            if (!updated) return NotFoundResponse("Producto no encontrado.");
            return OkMessage("Producto actualizado.");
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteProducto(int id)
        {
            var deleted = await _productoService.DeleteAsync(id);
            if (!deleted) return NotFoundResponse("Producto no encontrado.");
            return OkMessage("Producto eliminado.");
        }
    }
}
