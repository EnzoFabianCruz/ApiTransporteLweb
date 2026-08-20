using System;
using System.Collections.Generic;

namespace ApiTransporteLweb.Models.Dtos
{
    public class ParteParadaDto
    {
        public string? NumeroParte { get; set; }
        public DateTime? FechaParada { get; set; }
        public string? CodigoAnalitico { get; set; }
        public string? CodigoUnidad { get; set; }
        public string? Turno { get; set; }
        public string? SituacionParada { get; set; }
        public string? Supervisadopor { get; set; }

        public decimal? HoroInicial { get; set; }
        public decimal? HoroFinal { get; set; }
        public decimal? KmInicial { get; set; }
        public decimal? KmFinal { get; set; }
        public decimal? HoraInicial { get; set; }
        public decimal? HoraFinal { get; set; }

        public int? Qcombustible { get; set; }
        public decimal? Qhorormetro { get; set; }
        public DateTime? Qfechahora { get; set; }

        public string? Observacion { get; set; }

        public List<ParteParadaDetalleDto> Detalles { get; set; } = new List<ParteParadaDetalleDto>();
    }

    public class ParteParadaDetalleDto
    {
        public decimal? HoraInicial { get; set; }
        public decimal? HoraFinal { get; set; }
        public string? CodigoMotivo { get; set; }
        public string? Observacion { get; set; }
    }
}