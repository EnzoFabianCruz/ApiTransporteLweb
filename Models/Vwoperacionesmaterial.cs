namespace ApiTransporteLweb.Models
{
    // Entidad sin clave (Keyless) porque mapea una VISTA, no una tabla con PK.
    public class VwOperacionesMaterial
    {
        public string Codigom { get; set; } = null!;      // char/varchar
        public string Nombrem { get; set; } = null!;       // varchar
        public string Abreviatura { get; set; } = null!;   // varchar
    }
}