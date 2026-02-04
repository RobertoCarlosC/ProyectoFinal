namespace EnerGym.Api.Models
{
    public class Product
    {
        public int Id { get; set; }

        public required string Nombre { get; set; }
        public string? Descripcion { get; set; }
        public decimal Precio { get; set; }
        public int Stock { get; set; }
        public bool Activo { get; set; }

        // FK
        public int CategoryId { get; set; }
        public Category? Category { get; set; }
    }
}
