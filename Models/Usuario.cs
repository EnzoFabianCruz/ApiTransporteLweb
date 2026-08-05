namespace ApiTransporteLweb.Models
{
    public class Usuario
    {
        public int Id { get; set; }
        public string NombreUsuario { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string? Nombre { get; set; }
        public string? Email { get; set; }
        public bool Activo { get; set; }
        public DateTime FechaCreacion { get; set; }
        public string Rol { get; set; } = "Usuario";   // "Admin" o "Usuario"
    }
}