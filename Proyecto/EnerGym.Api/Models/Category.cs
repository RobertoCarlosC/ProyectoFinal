namespace EnerGym.Api.Models
{
    public class Category
    {
        public int Id { get; set; }

        public required string Nombre { get; set; }

        // Relaciones
        public List<Product>? Products { get; set; }
    }
}
