namespace EnerGym.Api.Models
{
    public class Cupon
    {
        public int IdCupon { get; set; }

        public string Codigo { get; set; } = string.Empty;
        public decimal Descuento { get; set; }

        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }

        public bool Activo { get; set; }

        public List<Pedido>? Pedidos { get; set; }
    }
}
