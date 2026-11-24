namespace ApiTransporteLweb.Models.Dtos
{
    // Resultado genérico de un reporte (el SP define las columnas)
    public class ReporteResultadoDto
    {
        public string Reporte { get; set; } = null!;
        public DateTime FechaDesde { get; set; }
        public DateTime FechaHasta { get; set; }
        public string? Turno { get; set; }
        public string? Formato { get; set; }
        public List<string> Columnas { get; set; } = new();
        public List<Dictionary<string, object?>> Filas { get; set; } = new();
        public int TotalFilas => Filas.Count;
    }
}
