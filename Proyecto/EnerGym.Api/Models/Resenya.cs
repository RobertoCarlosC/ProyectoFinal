namespace EnerGym.Api.Models
{
    public class Resenya
    {
        public int IdResena { get; set; }

        public int Calificacion { get; set; }
        public string Comentario { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }

        // FK Usuario
        public int IdUsuario { get; set; }
        public Usuario? Usuario { get; set; }

        // FK Producto
        public int IdProducto { get; set; }
        public Producto? Producto { get; set; }
    }
}
