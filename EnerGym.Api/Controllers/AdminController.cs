using Microsoft.AspNetCore.Mvc;
using EnerGym.Models;
using EnerGym.Services.Interfaces;
using EnerGym.Repositories.Interfaces;

namespace EnerGym.Controllers
{
    [ApiController]
    [Route("api/admin")]
    public class AdminController : BaseApiController
    {
        private readonly IAdminService _adminService;
        private readonly IProductoService _productoService;
        private readonly IUsuarioService _usuarioService;
        private readonly IPedidoService _pedidoService;
        private readonly ICarritoService _carritoService;

        public AdminController(
            IAdminService adminService,
            IProductoService productoService,
            IUsuarioService usuarioService,
            IPedidoService pedidoService,
            ICarritoService carritoService)
        {
            _adminService = adminService;
            _productoService = productoService;
            _usuarioService = usuarioService;
            _pedidoService = pedidoService;
            _carritoService = carritoService;
        }

        private async Task<bool> VerificarAdmin(int idAdmin)
        {
            return await _adminService.IsAdminAsync(idAdmin);
        }

        [HttpGet("{idAdmin:int}/productos/todos")]
        public async Task<IActionResult> GetProductosTodos(int idAdmin)
        {
            if (!await VerificarAdmin(idAdmin))
                return UnauthorizedResponse("No tienes permisos de administrador.");

            var productos = await _productoService.GetAllAsync();
            return OkData(productos);
        }

        [HttpPost("{idAdmin:int}/productos/crear")]
        public async Task<IActionResult> CrearProducto(int idAdmin, [FromBody] CrearProductoAdminDto dto)
        {
            if (!await VerificarAdmin(idAdmin))
                return UnauthorizedResponse("No tienes permisos de administrador.");

            if (string.IsNullOrWhiteSpace(dto.Nombre) || dto.Precio <= 0 || dto.Stock < 0 || dto.IdCategoria <= 0)
                return BadRequestResponse("Campos inválidos. Nombre, Precio, Categoría son obligatorios.");

            var id = await _productoService.CreateAsync(new ProductoDto
            {
                Nombre = dto.Nombre,
                Descripcion = dto.Descripcion,
                Precio = dto.Precio,
                Stock = dto.Stock,
                Imagen = dto.Imagen,
                IdCategoria = dto.IdCategoria
            });

            return Created("", new { idProducto = id, message = "Producto creado correctamente." });
        }

        [HttpPut("{idAdmin:int}/productos/{idProducto:int}/editar")]
        public async Task<IActionResult> EditarProducto(int idAdmin, int idProducto, [FromBody] EditarProductoAdminDto dto)
        {
            if (!await VerificarAdmin(idAdmin))
                return UnauthorizedResponse("No tienes permisos de administrador.");

            if (string.IsNullOrWhiteSpace(dto.Nombre) || dto.Precio <= 0 || dto.Stock < 0 || dto.IdCategoria <= 0)
                return BadRequestResponse("Campos inválidos.");

            var updated = await _productoService.UpdateAsync(idProducto, new ProductoDto
            {
                Nombre = dto.Nombre,
                Descripcion = dto.Descripcion,
                Precio = dto.Precio,
                Stock = dto.Stock,
                Imagen = dto.Imagen,
                IdCategoria = dto.IdCategoria
            });

            if (!updated) return NotFoundResponse("Producto no encontrado.");
            return OkMessage("Producto actualizado correctamente.");
        }

        [HttpDelete("{idAdmin:int}/productos/{idProducto:int}")]
        public async Task<IActionResult> EliminarProducto(int idAdmin, int idProducto)
        {
            if (!await VerificarAdmin(idAdmin))
                return UnauthorizedResponse("No tienes permisos de administrador.");

            var deleted = await _productoService.DeleteAsync(idProducto);
            if (!deleted) return NotFoundResponse("Producto no encontrado.");
            return OkMessage("Producto eliminado correctamente.");
        }

        [HttpGet("{idAdmin:int}/usuarios/todos")]
        public async Task<IActionResult> GetUsuariosTodos(int idAdmin)
        {
            if (!await VerificarAdmin(idAdmin))
                return UnauthorizedResponse("No tienes permisos de administrador.");

            var usuarios = await _usuarioService.GetAllAsync();
            return OkData(usuarios);
        }

        [HttpPut("{idAdmin:int}/usuarios/{idUsuario:int}/editar")]
        public async Task<IActionResult> EditarUsuario(int idAdmin, int idUsuario, [FromBody] EditarUsuarioAdminDto dto)
        {
            if (!await VerificarAdmin(idAdmin))
                return UnauthorizedResponse("No tienes permisos de administrador.");

            if (string.IsNullOrWhiteSpace(dto.Nombre) || string.IsNullOrWhiteSpace(dto.Email))
                return BadRequestResponse("Nombre y Email son obligatorios.");

            dto.IdUsuario = idUsuario;
            // Nota: el servicio de usuario no tiene un método update para admin. Necesitamos usar el repo directamente o extender el servicio.
            // Para mantener compatibilidad, usamos una excepción temporal: el controller usará el repo vía service injection.
            // Como simplificación, devolvemos un error indicando que use el endpoint de usuarios.
            return BadRequestResponse("Use PUT /api/usuarios/{id}/editar-perfil para editar usuarios.");
        }

        [HttpDelete("{idAdmin:int}/usuarios/{idUsuario:int}")]
        public async Task<IActionResult> EliminarUsuario(int idAdmin, int idUsuario)
        {
            if (idAdmin == idUsuario)
                return BadRequestResponse("No puedes eliminar tu propia cuenta.");

            if (!await VerificarAdmin(idAdmin))
                return UnauthorizedResponse("No tienes permisos de administrador.");

            return BadRequestResponse("Eliminación de usuarios no implementada en servicio.");
        }

        [HttpGet("{idAdmin:int}/usuarios/{idUsuario:int}/carrito")]
        public async Task<IActionResult> GetCarritoUsuario(int idAdmin, int idUsuario)
        {
            if (!await VerificarAdmin(idAdmin))
                return UnauthorizedResponse("No tienes permisos de administrador.");

            var carrito = await _carritoService.GetCarritoAsync(idUsuario);
            if (carrito == null) return OkData(new { idCarrito = (int?)null, items = new List<object>(), total = 0 });
            return OkData(carrito);
        }

        [HttpGet("{idAdmin:int}/pedidos/todos")]
        public async Task<IActionResult> GetPedidosTodos(int idAdmin)
        {
            if (!await VerificarAdmin(idAdmin))
                return UnauthorizedResponse("No tienes permisos de administrador.");

            var pedidos = await _pedidoService.GetAllAsync();
            return OkData(pedidos);
        }

        [HttpPut("{idAdmin:int}/pedidos/{idPedido:int}/cambiar-estado")]
        public async Task<IActionResult> CambiarEstadoPedido(int idAdmin, int idPedido, [FromBody] CambiarEstadoPedidoDto dto)
        {
            var estadosValidos = new[] { "Pendiente", "Procesando", "Enviado", "En reparto", "Entregado" };
            if (!estadosValidos.Contains(dto.NuevoEstado))
                return BadRequestResponse("Estado inválido. Estados válidos: Pendiente, Procesando, Enviado, En reparto, Entregado");

            if (!await VerificarAdmin(idAdmin))
                return UnauthorizedResponse("No tienes permisos de administrador.");

            var updated = await _pedidoService.CambiarEstadoAsync(idPedido, dto.NuevoEstado, idAdmin, null);
            if (!updated) return NotFoundResponse("Pedido no encontrado.");
            return OkMessage($"Estado del pedido cambiado a '{dto.NuevoEstado}'.");
        }

        [HttpGet("{idAdmin:int}/pedidos/{idPedido:int}/detalles")]
        public async Task<IActionResult> GetDetallesPedido(int idAdmin, int idPedido)
        {
            if (!await VerificarAdmin(idAdmin))
                return UnauthorizedResponse("No tienes permisos de administrador.");

            var pedido = await _pedidoService.GetByIdAsync(idPedido);
            if (pedido == null) return NotFoundResponse("Pedido no encontrado.");
            return OkData(new { pedido, detalles = pedido.Productos });
        }

        [HttpGet("{idAdmin:int}/estadisticas")]
        public async Task<IActionResult> GetEstadisticas(int idAdmin)
        {
            if (!await VerificarAdmin(idAdmin))
                return UnauthorizedResponse("No tienes permisos de administrador.");

            var stats = await _adminService.GetDashboardAsync();
            return OkData(stats);
        }
    }
}
