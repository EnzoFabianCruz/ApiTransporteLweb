namespace ApiTransporteLweb.Models
{
    public class PuestoTrabajo
    {
        public string CodigoEmp { get; set; } = null!;         // char(3)
        public string CodigoPuesto { get; set; } = null!;      // char(3)
        public string? DescripcionPuesto { get; set; }          // varchar(100)
    }
}