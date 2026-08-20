namespace ApiTransporteLweb.Models
{
    // Entidad sin clave (Keyless) porque mapea una VISTA, no una tabla con PK.
    public class VwOperacionesMotivo
    {
        public string Codigo { get; set; } = null!;   // char/varchar
        public string Motivo { get; set; } = null!;    // varchar
    }
}