namespace ApiTransporteLweb.Models.Dtos
{
    public class RegistrarUsuarioDto
    {
        public string NombreUsuario { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string? Nombre { get; set; }
        public string? Email { get; set; }
        public string? Rol { get; set; }        // "Admin" o "Usuario", opcional (default "Usuario")
        public bool Activo { get; set; } = true; // por defecto activo al crearse
    }
}