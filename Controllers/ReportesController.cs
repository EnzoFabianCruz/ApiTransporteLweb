using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ApiTransporteLweb.Data;
using ApiTransporteLweb.Helpers;
using ApiTransporteLweb.Models.Dtos;

namespace ApiTransporteLweb.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = ROL_ADMIN + "," + ROL_SUPERVISOR)]
    public class ReportesController : ControllerBase
    {
        private const string ROL_ADMIN = "Admin";
        private const string ROL_SUPERVISOR = "Supervisor";

        private static readonly string[] TURNOS = { "T", "D", "N" };   // T = Todos, D = Día, N = Noche
        private static readonly string[] FORMATOS_PARADA = { "parte", "detalle", "resumen" };

        private readonly ApplicationDbContext _context;

        public ReportesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET api/Reportes/horas?fechaDesde=2026-09-01&fechaHasta=2026-09-27
        // -> exec Sp_operaciones_trabajoxoperadorxturno '20260901','20260927'
        [HttpGet("horas")]
        public async Task<IActionResult> Horas(
            [FromQuery] DateTime fechaDesde,
            [FromQuery] DateTime fechaHasta,
            CancellationToken ct)
        {
            var error = ValidarFechas(fechaDesde, fechaHasta);
            if (error != null) return BadRequest(new { mensaje = error });

            return await Ejecutar("horas", "Sp_operaciones_trabajoxoperadorxturno",
                fechaDesde, fechaHasta, turno: null, formato: null, ct);
        }

        // GET api/Reportes/produccion?fechaDesde=2026-08-01&fechaHasta=2026-08-01&turno=N
        // -> exec Sp_operaciones_produccion_formato '20260801','20260801','N'
        [HttpGet("produccion")]
        public async Task<IActionResult> Produccion(
            [FromQuery] DateTime fechaDesde,
            [FromQuery] DateTime fechaHasta,
            [FromQuery] string turno = "T",
            CancellationToken ct = default)
        {
            var error = ValidarFechas(fechaDesde, fechaHasta) ?? ValidarTurno(ref turno);
            if (error != null) return BadRequest(new { mensaje = error });

            return await Ejecutar("produccion", "Sp_operaciones_produccion_formato",
                fechaDesde, fechaHasta, turno, formato: null, ct);
        }

        // GET api/Reportes/paradas?fechaDesde=...&fechaHasta=...&formato=parte|detalle|resumen&turno=T
        // formato=parte   -> Sp_operaciones_parada_formato  (desde, hasta, turno)
        // formato=detalle -> Sp_operaciones_paradadetalle   (desde, hasta)
        // formato=resumen -> Sp_operaciones_paradaresumen   (desde, hasta)
        [HttpGet("paradas")]
        public async Task<IActionResult> Paradas(
            [FromQuery] DateTime fechaDesde,
            [FromQuery] DateTime fechaHasta,
            [FromQuery] string formato = "parte",
            [FromQuery] string turno = "T",
            CancellationToken ct = default)
        {
            formato = (formato ?? "").Trim().ToLowerInvariant();
            if (!FORMATOS_PARADA.Contains(formato))
                return BadRequest(new { mensaje = "Formato inválido. Use: parte, detalle o resumen" });

            var error = ValidarFechas(fechaDesde, fechaHasta);
            if (error == null && formato == "parte")
                error = ValidarTurno(ref turno);
            if (error != null) return BadRequest(new { mensaje = error });

            var sp = formato switch
            {
                "parte" => "Sp_operaciones_parada_formato",
                "detalle" => "Sp_operaciones_paradadetalle",
                _ => "Sp_operaciones_paradaresumen"
            };

            // Solo el parte diario recibe turno; detalle y resumen reciben únicamente las fechas
            return await Ejecutar("paradas", sp, fechaDesde, fechaHasta,
                formato == "parte" ? turno : null, formato, ct);
        }

        private async Task<IActionResult> Ejecutar(
            string reporte, string sp, DateTime desde, DateTime hasta,
            string? turno, string? formato, CancellationToken ct)
        {
            try
            {
                var parametros = new List<string> { desde.ToString("yyyyMMdd"), hasta.ToString("yyyyMMdd") };
                if (turno != null) parametros.Add(turno);

                var (columnas, filas) = await ReporteSpHelper.EjecutarAsync(
                    _context, sp, parametros.ToArray(), ct);

                return Ok(new ReporteResultadoDto
                {
                    Reporte = reporte,
                    FechaDesde = desde.Date,
                    FechaHasta = hasta.Date,
                    Turno = turno,
                    Formato = formato,
                    Columnas = columnas,
                    Filas = filas
                });
            }
            catch (OperationCanceledException)
            {
                return StatusCode(499, new { mensaje = "Solicitud cancelada" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al generar el reporte", detalle = ex.Message });
            }
        }

        private static string? ValidarFechas(DateTime desde, DateTime hasta)
        {
            if (desde == default || hasta == default)
                return "Debe indicar fechaDesde y fechaHasta";
            if (desde.Date > hasta.Date)
                return "fechaDesde no puede ser mayor que fechaHasta";
            return null;
        }

        private static string? ValidarTurno(ref string turno)
        {
            turno = (turno ?? "").Trim().ToUpperInvariant();
            return TURNOS.Contains(turno) ? null : "Turno inválido. Use: T (todos), D (día) o N (noche)";
        }
    }
}
