using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ApiTransporteLweb.Data;

namespace ApiTransporteLweb.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ParteParadaController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ParteParadaController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET /api/ParteParada?busqueda=texto&fechaDesde=2026-01-01&fechaHasta=2026-01-31
        [HttpGet]
        public async Task<IActionResult> ObtenerParadas(
            [FromQuery] string? busqueda,
            [FromQuery] DateTime? fechaDesde,
            [FromQuery] DateTime? fechaHasta)
        {
            var query = _context.ParteParadas.AsQueryable();

            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                var texto = busqueda.Trim();
                query = query.Where(p =>
                    p.NumeroParada.Contains(texto) ||
                    (p.CodigoUnidad != null && p.CodigoUnidad.Contains(texto)) ||
                    (p.CodigoAnalitico != null && p.CodigoAnalitico.Contains(texto)) ||
                    (p.Operadorpor != null && p.Operadorpor.Contains(texto)) ||
                    (p.Supervisadopor != null && p.Supervisadopor.Contains(texto)) ||
                    (p.SituacionParada != null && p.SituacionParada.Contains(texto)));
            }

            if (fechaDesde.HasValue)
            {
                query = query.Where(p => p.FechaParada >= fechaDesde.Value.Date);
            }

            if (fechaHasta.HasValue)
            {
                var hasta = fechaHasta.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(p => p.FechaParada <= hasta);
            }

            var paradas = await query
                .OrderByDescending(p => p.FechaParada)
                .Select(p => new
                {
                    p.NumeroParada,
                    p.FechaParada,
                    p.CodigoUnidad,
                    p.CodigoAnalitico,
                    p.SituacionParada,
                    p.Operadorpor,
                    p.Supervisadopor,
                    p.Turno
                })
                .ToListAsync();

            return Ok(paradas);
        }

        // DELETE /api/ParteParada/{numeroParada} -> solo Admin
        [HttpDelete("{numeroParada}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Eliminar(string numeroParada)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var parada = await _context.ParteParadas
                    .FirstOrDefaultAsync(p => p.NumeroParada.Trim() == numeroParada.Trim());

                if (parada == null)
                    return NotFound(new { mensaje = "Parada no encontrada" });

                var detalles = _context.ParteParadaDetalles
                    .Where(d => d.NumeroParada.Trim() == numeroParada.Trim());
                _context.ParteParadaDetalles.RemoveRange(detalles);
                _context.ParteParadas.Remove(parada);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { mensaje = "Parada eliminada correctamente" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { mensaje = "Error al eliminar", detalle = ex.Message });
            }
        }
    }
}