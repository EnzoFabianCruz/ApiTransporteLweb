namespace ApiTransporteLweb.Models
{
    public class UnidadesTransporte
    {
        public string CodigoEmp { get; set; } = null!;         // char(3)
        public string CodigoUnidad { get; set; } = null!;      // char(6)
        public string? Volq { get; set; }                        // varchar(20)
        public string? Placa { get; set; }                       // varchar(20)
    }
}