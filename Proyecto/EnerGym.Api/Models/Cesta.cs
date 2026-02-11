namespace EnerGym.Api.Models
{
    public class Cesta
    {
        public int IdCesta { get; set; }

        public bool Persistente { get; set; }
        public DateTime FechaCreacion { get; set; }

        // FK Usuario (1,1)
        public int IdUsuario { get; set; }
        public Usuario? Usuario { get; set; }
    }
}
