using System;
using System.Collections.Generic;

namespace ApiTransporteLweb.Models
{
    public class ParteParada
    {
        public string CodigoEmp { get; set; } = null!;       // char(3)
        public string NumeroParada { get; set; } = null!;    // char(10)
        public string? NumeroParte { get; set; }               // char(10) - vincula con PARTETRABAJO.NumeroParte
        public DateTime? FechaParada { get; set; }
        public string? CodigoAnalitico { get; set; }          // char(6)
        public string? CodigoUnidad { get; set; }              // char(6)
        public decimal? IHorometro { get; set; }               // numeric(10,2)
        public decimal? ICombustible { get; set; }             // numeric(10)
        public decimal? IRegimen { get; set; }                 // numeric(10,2)
        public int? IReserva { get; set; }
        public decimal? FHorometro { get; set; }               // numeric(10,2)
        public decimal? FCombustible { get; set; }             // numeric(10)
        public decimal? FRegimen { get; set; }                 // numeric(10,2)
        public int? FReserva { get; set; }
        public int? QCombustible { get; set; }
        public decimal? QHorometro { get; set; }               // numeric(10,2)
        public DateTime? QFechaHora { get; set; }
        public string? Operadorpor { get; set; }               // char(6)
        public string? Supervisadopor { get; set; }            // char(6)
        public string? SituacionParada { get; set; }           // char(2)
        public DateTime FechaCreacion { get; set; }
        public string UsuarioCreacion { get; set; } = null!;
        public DateTime? FechaModificacion { get; set; }
        public string? UsuarioModificacion { get; set; }
        public string? Turno { get; set; }                     // char(1)

        // Un parteparada tiene muchos detalles
        public ICollection<ParteParadaDetalle> Detalles { get; set; } = new List<ParteParadaDetalle>();
    }
}