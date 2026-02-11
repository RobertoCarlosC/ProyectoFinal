namespace EnerGym.Api.Models
{
    public class Categoria
    {
        public int IdCategoria { get; set; }

        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;

        public List<Producto>? Productos { get; set; }
    }
}
