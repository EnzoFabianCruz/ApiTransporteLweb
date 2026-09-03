namespace ApiTransporteLweb.Models.Dtos
{
    public class SolicitarRecuperacionDto
{
    public string Email { get; set; } = null!;
}

public class RestablecerPasswordDto
{
    public string Token { get; set; } = null!;
    public string NuevaPassword { get; set; } = null!;
}
}
