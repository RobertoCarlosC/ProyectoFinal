namespace EnerGym.Api.Models
{
    public class Usuario
    {
        public int IdUsuario { get; set; }

        public string Nombre { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string TipoUsuario { get; set; } = string.Empty;

        public DateTime FechaRegistro { get; set; }

        // Relaciones
        public List<Pedido>? Pedidos { get; set; }
        public Cesta? Cesta { get; set; }
        public List<Resenya>? Resenas { get; set; }
    }
}
