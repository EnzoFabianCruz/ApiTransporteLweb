using System.ComponentModel.DataAnnotations.Schema;

namespace ApiTransporteLweb.Models
{
    public class Personal
    {
        public string CodigoPersonal { get; set; } = null!;   // char(6), PK
        public string? ApellidoPaterno { get; set; }            // varchar(35)
        public string? ApellidoMaterno { get; set; }            // varchar(35)
        public string? Nombres { get; set; }                    // varchar(50)

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public string? NombreCompleto { get; set; }              // computed varchar(123), la genera SQL Server solo
    }
}
