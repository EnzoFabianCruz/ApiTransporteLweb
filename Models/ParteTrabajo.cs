using System;
using System.Collections.Generic;

namespace ApiTransporteLweb.Models
{
    public class ParteTrabajo
    {
        public string CodigoEmp { get; set; } = null!;      // char(3)
        public string NumeroParte { get; set; } = null!;    // char(10)
        public DateTime? FechaParte { get; set; }
        public string? CodigoAnalitico { get; set; }         // char(6)
        public string? CodigoUnidad { get; set; }             // char(6)
        public decimal? HoroInicial { get; set; }             // numeric(20,2)
        public decimal? HoroFinal { get; set; }               // numeric(20,2)
        public decimal? KmInicial { get; set; }               // numeric(20,2)
        public decimal? KmFinal { get; set; }                 // numeric(20,2)
        public decimal? HoraInicial { get; set; }             // numeric(10,2)
        public decimal? Horafinal { get; set; }               // numeric(10,2)
        public string? Reportadopor { get; set; }             // char(6)
        public string? Supervisadopor { get; set; }           // char(6)
        public string? SituacionParte { get; set; }           // char(2)
        public DateTime FechaCreacion { get; set; }
        public string UsuarioCreacion { get; set; } = null!;
        public DateTime? FechaModificacion { get; set; }
        public string? UsuarioModificacion { get; set; }

        // Un parte tiene muchos detalles
        public ICollection<ParteTrabajoDetalle> Detalles { get; set; } = new List<ParteTrabajoDetalle>();
    }
}