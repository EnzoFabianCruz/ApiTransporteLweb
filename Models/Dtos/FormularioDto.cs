namespace ApiTransporteLweb.Models.Dtos
{
    public class FormularioDto
    {
        public DateTime? FechaParte { get; set; }
        public string? CodigoAnalitico { get; set; }
        public string? CodigoUnidad { get; set; }
        public decimal? HoroInicial { get; set; }
        public decimal? HoroFinal { get; set; }
        public decimal? KmInicial { get; set; }
        public decimal? KmFinal { get; set; }
        public decimal? HoraInicial { get; set; }
        public decimal? Horafinal { get; set; }
        public string? Reportadopor { get; set; }
        public string? Supervisadopor { get; set; }
        public string? SituacionParte { get; set; }
        public string? Turno { get; set; }
        public decimal? Combustible { get; set; }

        public List<DetalleDto> Detalles { get; set; } = new List<DetalleDto>();
    }

    public class DetalleDto
    {
        public decimal? DHoroFinal { get; set; }
        public decimal? DKmFinal { get; set; }
        public string? CodigoMaterial { get; set; }
        public string? CodigoMineral { get; set; }
        public string? CodigoOrigen { get; set; }
        public string? CodigoDestino { get; set; }
        public int? NumViajes { get; set; }
        public decimal? Peso { get; set; }
        public string? CodigoCiclo { get; set; }
        public string? ValorCiclo { get; set; }
        public string? Material { get; set; }
        public decimal? PHoras { get; set; }
    }
}