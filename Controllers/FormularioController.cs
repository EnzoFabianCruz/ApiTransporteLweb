using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ApiTransporteLweb.Data;
using ApiTransporteLweb.Models;
using ApiTransporteLweb.Models.Dtos;

namespace ApiTransporteLweb.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FormularioController : ControllerBase
    {
        private const string CODIGO_EMP_FIJO = "001";
        private const string ROL_ADMIN = "Admin";

        private readonly ApplicationDbContext _context;

        public FormularioController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET /api/Formulario?busqueda=texto&fechaDesde=2026-01-01&fechaHasta=2026-01-31
        [HttpGet]
        public async Task<IActionResult> ObtenerPartes(
            [FromQuery] string? busqueda,
            [FromQuery] DateTime? fechaDesde,
            [FromQuery] DateTime? fechaHasta)
        {
            var usuarioActual = User.FindFirstValue(ClaimTypes.Name);
            var esAdmin = User.IsInRole(ROL_ADMIN);

            var query = _context.ParteTrabajos.AsQueryable();

            // Un operador (rol distinto de Admin) solo ve lo que él mismo creó
            if (!esAdmin)
            {
                query = query.Where(p => p.UsuarioCreacion != null && p.UsuarioCreacion.Trim() == usuarioActual);
            }

            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                var texto = busqueda.Trim();
                query = query.Where(p =>
                    p.NumeroParte.Contains(texto) ||
                    (p.CodigoUnidad != null && p.CodigoUnidad.Contains(texto)) ||
                    (p.CodigoAnalitico != null && p.CodigoAnalitico.Contains(texto)) ||
                    (p.Reportadopor != null && p.Reportadopor.Contains(texto)) ||
                    (p.Supervisadopor != null && p.Supervisadopor.Contains(texto)) ||
                    (p.SituacionParte != null && p.SituacionParte.Contains(texto)));
            }

            if (fechaDesde.HasValue)
            {
                query = query.Where(p => p.FechaParte >= fechaDesde.Value.Date);
            }

            if (fechaHasta.HasValue)
            {
                // Se incluye el día completo de "hasta" (hasta las 23:59:59)
                var hasta = fechaHasta.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(p => p.FechaParte <= hasta);
            }

            var partes = await query
                .OrderByDescending(p => p.FechaParte)
                .Select(p => new
                {
                    p.NumeroParte,
                    p.FechaParte,
                    p.CodigoUnidad,
                    p.CodigoAnalitico,
                    p.SituacionParte,
                    p.Reportadopor,
                    p.Supervisadopor,
                    p.HoraInicial,
                    p.Horafinal,
                    p.Turno
                })
                .ToListAsync();

            return Ok(partes);
        }

        // GET /api/Formulario/siguiente-numero -> para mostrarlo en el formulario antes de guardar
        [HttpGet("siguiente-numero")]
        public async Task<IActionResult> ObtenerSiguienteNumero()
        {
            var numero = await GenerarSiguienteNumeroParte();
            return Ok(new { numeroParte = numero });
        }

        // GET /api/Formulario/{numeroParte} -> cabecera + detalles, para Consultar/Modificar
        [HttpGet("{numeroParte}")]
        public async Task<IActionResult> ObtenerParte(string numeroParte)
        {
            var parte = await _context.ParteTrabajos
                .FirstOrDefaultAsync(p => p.NumeroParte.Trim() == numeroParte.Trim());

            if (parte == null)
                return NotFound(new { mensaje = "Parte no encontrado" });

            if (!TienePermisoSobre(parte))
                return Forbid();

            var detalles = await _context.ParteTrabajoDetalles
                .Where(d => d.NumeroParte.Trim() == numeroParte.Trim())
                .OrderBy(d => d.NumeroLinea)
                .ToListAsync();

            return Ok(new
            {
                parte.NumeroParte,
                parte.FechaParte,
                parte.CodigoAnalitico,
                parte.CodigoUnidad,
                parte.HoroInicial,
                parte.HoroFinal,
                parte.KmInicial,
                parte.KmFinal,
                parte.HoraInicial,
                parte.Horafinal,
                parte.Reportadopor,
                parte.Supervisadopor,
                parte.SituacionParte,
                parte.Turno,
                parte.Combustible,
                Detalles = detalles.Select(d => new
                {
                    d.DHoroFinal,
                    d.DKmFinal,
                    d.CodigoMaterial,
                    d.CodigoMineral,
                    d.CodigoOrigen,
                    d.CodigoDestino,
                    d.NumViajes,
                    d.Peso,
                    d.CodigoCiclo,
                    d.ValorCiclo,
                    d.Material,
                    d.PHoras
                })
            });
        }

        // POST /api/Formulario/registrar -> crear
        [HttpPost("registrar")]
        public async Task<IActionResult> Registrar([FromBody] FormularioDto dto)
        {
            if (dto.Detalles == null || dto.Detalles.Count == 0)
                return BadRequest(new { mensaje = "Debe registrar al menos un detalle (viaje)" });

            var usuarioActual = User.FindFirstValue(ClaimTypes.Name) ?? "sistema";

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var numeroParte = await GenerarSiguienteNumeroParte();

                var parte = new ParteTrabajo
                {
                    CodigoEmp = CODIGO_EMP_FIJO,
                    NumeroParte = numeroParte,
                    FechaParte = dto.FechaParte,
                    CodigoAnalitico = dto.CodigoAnalitico,
                    CodigoUnidad = dto.CodigoUnidad,
                    HoroInicial = dto.HoroInicial,
                    HoroFinal = dto.HoroFinal,
                    KmInicial = dto.KmInicial,
                    KmFinal = dto.KmFinal,
                    HoraInicial = dto.HoraInicial,
                    Horafinal = dto.Horafinal,
                    Reportadopor = dto.Reportadopor,
                    Supervisadopor = dto.Supervisadopor,
                    SituacionParte = dto.SituacionParte,
                    Turno = dto.Turno,
                    Combustible = dto.Combustible,
                    FechaCreacion = DateTime.Now,
                    UsuarioCreacion = usuarioActual
                };
                _context.ParteTrabajos.Add(parte);
                await _context.SaveChangesAsync();

                int numeroLinea = 1;
                foreach (var det in dto.Detalles)
                {
                    _context.ParteTrabajoDetalles.Add(new ParteTrabajoDetalle
                    {
                        CodigoEmp = CODIGO_EMP_FIJO,
                        NumeroParte = numeroParte,
                        NumeroLinea = numeroLinea,
                        DHoroFinal = det.DHoroFinal,
                        DKmFinal = det.DKmFinal,
                        CodigoMaterial = det.CodigoMaterial,
                        CodigoMineral = det.CodigoMineral,
                        CodigoOrigen = det.CodigoOrigen,
                        CodigoDestino = det.CodigoDestino,
                        NumViajes = det.NumViajes,
                        Peso = det.Peso,
                        CodigoCiclo = det.CodigoCiclo,
                        ValorCiclo = det.ValorCiclo,
                        Material = det.Material,
                        PHoras = det.PHoras,
                        FechaCreacion = DateTime.Now,
                        UsuarioCreacion = usuarioActual
                    });
                    numeroLinea++;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { mensaje = "Parte de trabajo registrado correctamente", numeroParte });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { mensaje = "Error al registrar", detalle = ex.Message });
            }
        }

        // PUT /api/Formulario/{numeroParte} -> modificar cabecera + reemplazar detalles
        [HttpPut("{numeroParte}")]
        public async Task<IActionResult> Modificar(string numeroParte, [FromBody] FormularioDto dto)
        {
            if (dto.Detalles == null || dto.Detalles.Count == 0)
                return BadRequest(new { mensaje = "Debe tener al menos un detalle (viaje)" });

            var usuarioActual = User.FindFirstValue(ClaimTypes.Name) ?? "sistema";

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var parte = await _context.ParteTrabajos
                    .FirstOrDefaultAsync(p => p.NumeroParte.Trim() == numeroParte.Trim());

                if (parte == null)
                    return NotFound(new { mensaje = "Parte no encontrado" });

                if (!TienePermisoSobre(parte))
                    return Forbid();

                parte.FechaParte = dto.FechaParte;
                parte.CodigoAnalitico = dto.CodigoAnalitico;
                parte.CodigoUnidad = dto.CodigoUnidad;
                parte.HoroInicial = dto.HoroInicial;
                parte.HoroFinal = dto.HoroFinal;
                parte.KmInicial = dto.KmInicial;
                parte.KmFinal = dto.KmFinal;
                parte.HoraInicial = dto.HoraInicial;
                parte.Horafinal = dto.Horafinal;
                parte.Reportadopor = dto.Reportadopor;
                parte.Supervisadopor = dto.Supervisadopor;
                parte.SituacionParte = dto.SituacionParte;
                parte.Turno = dto.Turno;
                parte.Combustible = dto.Combustible;
                parte.FechaModificacion = DateTime.Now;
                parte.UsuarioModificacion = usuarioActual;

                // Reemplazar todos los detalles: borrar los viejos, insertar los nuevos
                var detallesViejos = _context.ParteTrabajoDetalles
                    .Where(d => d.NumeroParte.Trim() == numeroParte.Trim());
                _context.ParteTrabajoDetalles.RemoveRange(detallesViejos);
                await _context.SaveChangesAsync();

                int numeroLinea = 1;
                foreach (var det in dto.Detalles)
                {
                    _context.ParteTrabajoDetalles.Add(new ParteTrabajoDetalle
                    {
                        CodigoEmp = CODIGO_EMP_FIJO,
                        NumeroParte = parte.NumeroParte,
                        NumeroLinea = numeroLinea,
                        DHoroFinal = det.DHoroFinal,
                        DKmFinal = det.DKmFinal,
                        CodigoMaterial = det.CodigoMaterial,
                        CodigoMineral = det.CodigoMineral,
                        CodigoOrigen = det.CodigoOrigen,
                        CodigoDestino = det.CodigoDestino,
                        NumViajes = det.NumViajes,
                        Peso = det.Peso,
                        CodigoCiclo = det.CodigoCiclo,
                        ValorCiclo = det.ValorCiclo,
                        Material = det.Material,
                        PHoras = det.PHoras,
                        FechaCreacion = DateTime.Now,
                        UsuarioCreacion = usuarioActual
                    });
                    numeroLinea++;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { mensaje = "Parte actualizado correctamente" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { mensaje = "Error al actualizar", detalle = ex.Message });
            }
        }

        // DELETE /api/Formulario/{numeroParte} -> solo Admin
        [HttpDelete("{numeroParte}")]
        [Authorize(Roles = ROL_ADMIN)]
        public async Task<IActionResult> Eliminar(string numeroParte)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var parte = await _context.ParteTrabajos
                    .FirstOrDefaultAsync(p => p.NumeroParte.Trim() == numeroParte.Trim());

                if (parte == null)
                    return NotFound(new { mensaje = "Parte no encontrado" });

                var detalles = _context.ParteTrabajoDetalles
                    .Where(d => d.NumeroParte.Trim() == numeroParte.Trim());
                _context.ParteTrabajoDetalles.RemoveRange(detalles);
                _context.ParteTrabajos.Remove(parte);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { mensaje = "Parte eliminado correctamente" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { mensaje = "Error al eliminar", detalle = ex.Message });
            }
        }

        // Un Admin puede ver/editar cualquier parte; un operador solo el suyo propio
        private bool TienePermisoSobre(ParteTrabajo parte)
        {
            if (User.IsInRole(ROL_ADMIN))
                return true;

            var usuarioActual = User.FindFirstValue(ClaimTypes.Name);
            return parte.UsuarioCreacion != null && parte.UsuarioCreacion.Trim() == usuarioActual;
        }

        // Método auxiliar: calcula el siguiente número de parte (8 dígitos, con ceros a la izquierda)
        private async Task<string> GenerarSiguienteNumeroParte()
        {
            var numerosExistentes = await _context.ParteTrabajos
                .Select(p => p.NumeroParte)
                .ToListAsync();

            int siguiente = 1;

            if (numerosExistentes.Any())
            {
                var maximo = numerosExistentes
                    .Select(n => n.Trim())
                    .Where(n => int.TryParse(n, out _))
                    .Select(int.Parse)
                    .DefaultIfEmpty(0)
                    .Max();

                siguiente = maximo + 1;
            }

            return siguiente.ToString("D10"); // 1 -> "00000001"
        }
    }
}