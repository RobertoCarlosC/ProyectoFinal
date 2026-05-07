namespace EnerGym.Repositories.Interfaces
{
    public interface IMensajeRepository
    {
        Task<int> CrearAsync(CrearMensajeDto dto);
        Task<List<MensajeDto>> GetAllAsync();
        Task<List<MensajeDto>> GetByUsuarioAsync(int idUsuario);
        Task<bool> ResponderAsync(int idMensaje, string? respuesta);
        Task<bool> MarcarLeidoAsync(int idMensaje);
        Task<bool> MarcarRespondidoAsync(int idMensaje);
        Task<bool> EliminarAsync(int idMensaje);
        Task<int> CountNoLeidosAsync();
    }

    public class CrearMensajeDto
    {
        public int? IdUsuario { get; set; }
        public string Nombre { get; set; } = "";
        public string Email { get; set; } = "";
        public string Asunto { get; set; } = "";
        public string Mensaje { get; set; } = "";
    }

    public class MensajeDto
    {
        public int IdMensaje { get; set; }
        public int? IdUsuario { get; set; }
        public string Nombre { get; set; } = "";
        public string Email { get; set; } = "";
        public string Asunto { get; set; } = "";
        public string Mensaje { get; set; } = "";
        public string Fecha { get; set; } = "";
        public bool Leido { get; set; }
        public bool Respondido { get; set; }
        public string? Respuesta { get; set; }
    }
}
