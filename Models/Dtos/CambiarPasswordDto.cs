namespace ApiTransporteLweb.Models.Dtos
{
    public class CambiarPasswordDto
    {
        public string? PasswordActual { get; set; }   // requerida solo si NO eres admin
        public string NuevaPassword { get; set; } = null!;
    }
}