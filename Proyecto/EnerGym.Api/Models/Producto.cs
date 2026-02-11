namespace EnerGym.Api.Models
{
    public class Producto
    {
        public int IdProducto { get; set; }

        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public decimal Precio { get; set; }
        public int Stock { get; set; }

        // FK Categoria (1,1)
        public int IdCategoria { get; set; }
        public Categoria? Categoria { get; set; }

        public List<Pedido>? Pedidos { get; set; }
        public List<Resenya>? Resenas { get; set; }
    }
}
