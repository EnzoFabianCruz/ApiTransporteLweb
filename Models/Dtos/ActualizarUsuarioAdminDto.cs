namespace ApiTransporteLweb.Models.Dtos
{
    public class ActualizarUsuarioAdminDto
    {
        public string NombreUsuario { get; set; } = null!;
        public string? Nombre { get; set; }
        public string? Email { get; set; }
        public string Rol { get; set; } = null!;
        public string? CodigoPersonal { get; set; }
        public bool Activo { get; set; } = true;
        public string? NuevaPassword { get; set; }   // opcional: si viene, se resetea
    }
}