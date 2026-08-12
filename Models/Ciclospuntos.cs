namespace ApiTransporteLweb.Models
{
    public class CiclosPuntos
    {
        public string CodigoEmp { get; set; } = null!;        // char(3)
        public string CodigoPunto { get; set; } = null!;       // char(6)
        public string? NombrePunto { get; set; }                // varchar(100)
    }
}