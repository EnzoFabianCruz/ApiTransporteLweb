namespace ApiTransporteLweb.Models
{
    public class ParteTrabajoDetalle
    {
        public string CodigoEmp { get; set; } = null!;       // char(3)
        public string NumeroParte { get; set; } = null!;     // char(10)
        public int NumeroLinea { get; set; }                  // int

        public decimal? DHoroFinal { get; set; }              // numeric(20,2)
        public decimal? DKmFinal { get; set; }                // numeric(20,2)
        public string? CodigoMaterial { get; set; }           // char(3)
        public string? CodigoMineral { get; set; }            // varchar(30)
        public string? CodigoOrigen { get; set; }             // char(6)
        public string? CodigoDestino { get; set; }            // char(6)
        public int? NumViajes { get; set; }
        public decimal? Peso { get; set; }                    // numeric(10,3)
        public DateTime FechaCreacion { get; set; }
        public string? UsuarioCreacion { get; set; }
        public DateTime? FechaModificacion { get; set; }
        public string? UsuarioModificacion { get; set; }
        public string? CodigoCiclo { get; set; }              // char(6)
        public string? ValorCiclo { get; set; }               // varchar(50)
        public string? Material { get; set; }                 // varchar(10)
        public decimal? PHoras { get; set; }                  // numeric(20,2)

        // Propiedad de navegación: el detalle pertenece a un parte
        public ParteTrabajo ParteTrabajo { get; set; } = null!;
    }
}