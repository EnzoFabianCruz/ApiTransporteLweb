namespace ApiTransporteLweb.Models
{
    public class UnidadesCiclos
    {
        public string CodigoEmp { get; set; } = null!;        // char(3)
        public string CodigoCiclo { get; set; } = null!;       // char(6)
        public string? CodigoOrigen { get; set; }               // char(6), relaciona con CiclosPuntos.CodigoPunto
        public string? CodigoDestino { get; set; }              // char(6), relaciona con CiclosPuntos.CodigoPunto
        public string? Valor { get; set; }                      // varchar(100), texto que se muestra como opción
    }
}