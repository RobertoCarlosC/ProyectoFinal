namespace EnerGym.Models
{
    

    public class Usuario
    {
        public int IdUsuario { get; set; }
        public string Nombre { get; set; } = "";
        public string Email { get; set; } = "";
        public string PasswordHash { get; set; } = "";   
        public int IdRol { get; set; }                   
        public DateTime FechaRegistro { get; set; }
    }

    public class Producto
    {
        public int IdProducto { get; set; }
        public string Nombre { get; set; } = "";
        public string Descripcion { get; set; } = "";
        public decimal Precio { get; set; }
        public int Stock { get; set; }
        public string Imagen { get; set; } = "";
        public int IdCategoria { get; set; }
        public string? NombreCategoria { get; set; }
        public bool TieneLike { get; set; }
    }

    public class Categoria
    {
        public int IdCategoria { get; set; }
        public string Nombre { get; set; } = "";
        public string Descripcion { get; set; } = "";
    }

    public class Carrito
    {
        public int IdCarrito { get; set; }
        public int IdUsuario { get; set; }
        public DateTime FechaCreacion { get; set; }
        public List<CarritoItem> Items { get; set; } = new();
        public decimal Total { get; set; }
    }

    public class CarritoItem
    {
        public int Id { get; set; }
        public int IdCarrito { get; set; }
        public int IdProducto { get; set; }
        public string NombreProducto { get; set; } = "";
        public string Imagen { get; set; } = "";
        public decimal Precio { get; set; }
        public int Cantidad { get; set; }
        public decimal Subtotal { get; set; }
    }

    public class Pedido
    {
        public int IdPedido { get; set; }
        public int IdUsuario { get; set; }
        public DateTime Fecha { get; set; }
        public decimal Total { get; set; }
        public string Estado { get; set; } = "Pendiente";  
        public List<PedidoDetalle> Detalles { get; set; } = new();
    }

    public class PedidoDetalle
    {
        public int IdDetalle { get; set; }
        public int IdPedido { get; set; }
        public int IdProducto { get; set; }
        public string NombreProducto { get; set; } = "";
        public int Cantidad { get; set; }
        public decimal Precio { get; set; }
    }

    
    public class RegisterDto
    {
        public string Nombre { get; set; } = "";
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";   
    }

    
    public class LoginDto
    {
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";   
    }

    
    public class LoginResponse
    {
        public int IdUsuario { get; set; }
        public string Nombre { get; set; } = "";
        public string Email { get; set; } = "";
        public int IdRol { get; set; }
    }

    public class AddCarritoDto
    {
        public int IdUsuario { get; set; }
        public int IdProducto { get; set; }
        public int Cantidad { get; set; }
    }

    public class UpdateCantidadDto
    {
        public int Cantidad { get; set; }
    }

    public class ConfirmarPedidoDto
    {
        public int IdUsuario { get; set; }
    }

    public class LikeDto
    {
        public int IdUsuario { get; set; }
        public int IdProducto { get; set; }
    }

    public class ProductoDto
    {
        public string Nombre { get; set; } = "";
        public string Descripcion { get; set; } = "";
        public decimal Precio { get; set; }
        public int Stock { get; set; }
        public string Imagen { get; set; } = "";
        public int IdCategoria { get; set; }
    }

    
    public class CambiarContraseñaDto
    {
        public int IdUsuario { get; set; }
        public string ContraseñaActual { get; set; } = "";
        public string ContraseñaNueva { get; set; } = "";
        public string ConfirmarContraseña { get; set; } = "";
    }

    public class EditarPerfilDto
    {
        public int IdUsuario { get; set; }
        public string Nombre { get; set; } = "";
        public string Email { get; set; } = "";
        public string? Telefono { get; set; }
        public string? Direccion { get; set; }
        public string? Ciudad { get; set; }
        public string? CodigoPostal { get; set; }
        public string? FotoPerfil { get; set; }
    }

    public class EditarProductoAdminDto
    {
        public int IdProducto { get; set; }
        public string Nombre { get; set; } = "";
        public string Descripcion { get; set; } = "";
        public decimal Precio { get; set; }
        public int Stock { get; set; }
        public string Imagen { get; set; } = "";
        public int IdCategoria { get; set; }
    }

    public class CrearProductoAdminDto
    {
        public string Nombre { get; set; } = "";
        public string Descripcion { get; set; } = "";
        public decimal Precio { get; set; }
        public int Stock { get; set; }
        public string Imagen { get; set; } = "";
        public int IdCategoria { get; set; }
    }

    public class CambiarEstadoPedidoDto
    {
        public int IdPedido { get; set; }
        public string NuevoEstado { get; set; } = "";  
    }

    public class EditarUsuarioAdminDto
    {
        public int IdUsuario { get; set; }
        public string Nombre { get; set; } = "";
        public string Email { get; set; } = "";
        public int IdRol { get; set; }
        public string? Telefono { get; set; }
        public string? Direccion { get; set; }
        public string? Ciudad { get; set; }
        public string? CodigoPostal { get; set; }
    }

    public class ResumenPedidoDto
    {
        public int IdPedido { get; set; }
        public DateTime Fecha { get; set; }
        public decimal Total { get; set; }
        public string Estado { get; set; } = "";
        public int CantidadProductos { get; set; }
    }
}

