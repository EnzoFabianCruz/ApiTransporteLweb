using System;

namespace ApiTransporteLweb.Models
{
    public class ParteParadaDetalle
    {
        public string CodigoEmp { get; set; } = null!;        // char(3)
        public string NumeroParada { get; set; } = null!;     // char(10)
        public int NumeroLinea { get; set; }                   // int

        public decimal? HoraInicial { get; set; }              // numeric(10,2)
        public decimal? HoraFinal { get; set; }                // numeric(10,2)
        public string? CodigoMotivo { get; set; }              // char(6)
        public string? Observacion { get; set; }                // varchar(250) -> columna real "observaciond"

        public DateTime FechaCreacion { get; set; }
        public string? UsuarioCreacion { get; set; }
        public DateTime? FechaModificacion { get; set; }
        public string? UsuarioModificacion { get; set; }

        // Propiedad de navegación: el detalle pertenece a un parteparada
        public ParteParada ParteParada { get; set; } = null!;
    }
}