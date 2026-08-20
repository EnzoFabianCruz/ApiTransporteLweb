using System;
using System.Collections.Generic;

namespace ApiTransporteLweb.Models
{
    public class ParteParada
    {
        public string CodigoEmp { get; set; } = null!;
        public string NumeroParada { get; set; } = null!;
        public string? NumeroParte { get; set; }
        public DateTime? FechaParada { get; set; }
        public string? CodigoAnalitico { get; set; }
        public string? CodigoUnidad { get; set; }

        public decimal? HoroInicial { get; set; }
        public decimal? HoroFinal { get; set; }
        public decimal? KmInicial { get; set; }
        public decimal? KmFinal { get; set; }
        public decimal? HoraInicial { get; set; }
        public decimal? HoraFinal { get; set; }

        public int? Qcombustible { get; set; }
        public decimal? Qhorormetro { get; set; }
        public DateTime? Qfechahora { get; set; }

        public string? Operadorpor { get; set; }
        public string? Supervisadopor { get; set; }
        public string? SituacionParada { get; set; }
        public DateTime FechaCreacion { get; set; }
        public string UsuarioCreacion { get; set; } = null!;
        public DateTime? FechaModificacion { get; set; }
        public string? UsuarioModificacion { get; set; }
        public string? Turno { get; set; }
        public string? Observacion { get; set; }

        public ICollection<ParteParadaDetalle> Detalles { get; set; } = new List<ParteParadaDetalle>();
    }
}