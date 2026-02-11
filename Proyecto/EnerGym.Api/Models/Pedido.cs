namespace EnerGym.Api.Models
{
    public class Pedido
    {
        public int IdPedido { get; set; }

        public DateTime Fecha { get; set; }
        public string Estado { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public DateTime FechaHora { get; set; }

        // FK Usuario
        public int IdUsuario { get; set; }
        public Usuario? Usuario { get; set; }

        // FK Cupón (0,1)
        public int? IdCupon { get; set; }
        public Cupon? Cupon { get; set; }

        // Relación muchos a muchos con Producto
        public List<Producto>? Productos { get; set; }
    }
}
