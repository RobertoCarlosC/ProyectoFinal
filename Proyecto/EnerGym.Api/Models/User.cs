namespace EnerGym.Api.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string TipoUsuario { get; set; } // admin | user
    }
}
