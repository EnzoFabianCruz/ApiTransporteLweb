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
    public class ParteParadaController : ControllerBase
    {
        private const string CODIGO_EMP_FIJO = "001";
        private const string ROL_ADMIN = "Admin";
        private const string ROL_SUPERVISOR = "Supervisor";               // ← nuevo

        private readonly ApplicationDbContext _context;

        public ParteParadaController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerParadas(
            [FromQuery] string? busqueda,
            [FromQuery] DateTime? fechaDesde,
            [FromQuery] DateTime? fechaHasta)
        {
            var usuarioActual = User.FindFirstValue(ClaimTypes.Name);
            var esAdmin = User.IsInRole(ROL_ADMIN);
            var esSupervisor = User.IsInRole(ROL_SUPERVISOR);                 // ← nuevo
            var codigoPersonalActual = User.FindFirstValue("CodigoPersonal"); // ← nuevo

            var query = _context.ParteParadas.AsQueryable();

            // Las paradas anuladas (situación 90) nunca deben mostrarse en la lista
            query = query.Where(p => p.SituacionParada == null || p.SituacionParada.Trim() != "90");

            if (esSupervisor)                                                 // ← nuevo bloque
            {
                query = query.Where(p =>
                    codigoPersonalActual != null &&
                    p.Supervisadopor != null &&
                    p.Supervisadopor.Trim() == codigoPersonalActual);
            }
            else if (!esAdmin)
            {
                // Un operador (rol distinto de Admin y Supervisor) solo ve lo que él mismo creó
                query = query.Where(p => p.UsuarioCreacion != null && p.UsuarioCreacion.Trim() == usuarioActual);
            }

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
                    p.NumeroParte,
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

        [HttpGet("siguiente-numero")]
        public async Task<IActionResult> ObtenerSiguienteNumero()
        {
            var numero = await GenerarSiguienteNumeroParada();
            return Ok(new { numeroParada = numero });
        }

        [HttpGet("{numeroParada}")]
        public async Task<IActionResult> ObtenerParada(string numeroParada)
        {
            var parada = await _context.ParteParadas
                .FirstOrDefaultAsync(p => p.NumeroParada.Trim() == numeroParada.Trim());

            if (parada == null)
                return NotFound(new { mensaje = "Parada no encontrada" });

            if (!TienePermisoSobre(parada))
                return Forbid();

            var detalles = await _context.ParteParadaDetalles
                .Where(d => d.NumeroParada.Trim() == numeroParada.Trim())
                .OrderBy(d => d.NumeroLinea)
                .ToListAsync();

            return Ok(new
            {
                parada.NumeroParada,
                parada.NumeroParte,
                parada.FechaParada,
                parada.CodigoAnalitico,
                parada.CodigoUnidad,
                parada.HoroInicial,
                parada.HoroFinal,
                parada.KmInicial,
                parada.KmFinal,
                parada.HoraInicial,
                parada.HoraFinal,
                parada.Qcombustible,
                parada.Qhorormetro,
                parada.Qfechahora,
                parada.Operadorpor,
                parada.Supervisadopor,
                parada.SituacionParada,
                parada.Turno,
                parada.Observacion,
                Detalles = detalles.Select(d => new
                {
                    d.HoraInicial,
                    d.HoraFinal,
                    d.CodigoMotivo,
                    d.Observacion
                })
            });
        }

        [HttpPost("registrar")]
        public async Task<IActionResult> Registrar([FromBody] ParteParadaDto dto)
        {
            if (dto.Detalles == null || dto.Detalles.Count == 0)
                return BadRequest(new { mensaje = "Debe registrar al menos un detalle (parada)" });

            var usuarioActual = User.FindFirstValue(ClaimTypes.Name) ?? "sistema";

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var numeroParada = await GenerarSiguienteNumeroParada();

                var parada = new ParteParada
                {
                    CodigoEmp = CODIGO_EMP_FIJO,
                    NumeroParada = numeroParada,
                    NumeroParte = dto.NumeroParte,
                    FechaParada = dto.FechaParada,
                    CodigoAnalitico = dto.CodigoAnalitico,
                    CodigoUnidad = dto.CodigoUnidad,
                    HoroInicial = dto.HoroInicial,
                    HoroFinal = dto.HoroFinal,
                    KmInicial = dto.KmInicial,
                    KmFinal = dto.KmFinal,
                    HoraInicial = dto.HoraInicial,
                    HoraFinal = dto.HoraFinal,
                    Qcombustible = dto.Qcombustible,
                    Qhorormetro = dto.Qhorormetro,
                    Qfechahora = dto.Qfechahora,
                    Supervisadopor = dto.Supervisadopor,
                    SituacionParada = dto.SituacionParada,
                    Turno = dto.Turno,
                    Observacion = dto.Observacion,
                    FechaCreacion = DateTime.Now,
                    UsuarioCreacion = usuarioActual
                };
                _context.ParteParadas.Add(parada);
                await _context.SaveChangesAsync();

                int numeroLinea = 1;
                foreach (var det in dto.Detalles)
                {
                    _context.ParteParadaDetalles.Add(new ParteParadaDetalle
                    {
                        CodigoEmp = CODIGO_EMP_FIJO,
                        NumeroParada = numeroParada,
                        NumeroLinea = numeroLinea,
                        HoraInicial = det.HoraInicial,
                        HoraFinal = det.HoraFinal,
                        CodigoMotivo = det.CodigoMotivo,
                        Observacion = det.Observacion,
                        FechaCreacion = DateTime.Now,
                        UsuarioCreacion = usuarioActual
                    });
                    numeroLinea++;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { mensaje = "Parte de parada registrado correctamente", numeroParada });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { mensaje = "Error al registrar", detalle = ex.Message });
            }
        }

        [HttpPut("{numeroParada}")]
        public async Task<IActionResult> Modificar(string numeroParada, [FromBody] ParteParadaDto dto)
        {
            if (dto.Detalles == null || dto.Detalles.Count == 0)
                return BadRequest(new { mensaje = "Debe tener al menos un detalle (parada)" });

            var usuarioActual = User.FindFirstValue(ClaimTypes.Name) ?? "sistema";

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var parada = await _context.ParteParadas
                    .FirstOrDefaultAsync(p => p.NumeroParada.Trim() == numeroParada.Trim());

                if (parada == null)
                    return NotFound(new { mensaje = "Parada no encontrada" });

                if (!TienePermisoSobre(parada))
                    return Forbid();

                parada.FechaParada = dto.FechaParada;
                parada.NumeroParte = dto.NumeroParte;
                parada.CodigoAnalitico = dto.CodigoAnalitico;
                parada.CodigoUnidad = dto.CodigoUnidad;
                parada.HoroInicial = dto.HoroInicial;
                parada.HoroFinal = dto.HoroFinal;
                parada.KmInicial = dto.KmInicial;
                parada.KmFinal = dto.KmFinal;
                parada.HoraInicial = dto.HoraInicial;
                parada.HoraFinal = dto.HoraFinal;
                parada.Qcombustible = dto.Qcombustible;
                parada.Qhorormetro = dto.Qhorormetro;
                parada.Qfechahora = dto.Qfechahora;
                parada.Supervisadopor = dto.Supervisadopor;
                parada.SituacionParada = dto.SituacionParada;
                parada.Turno = dto.Turno;
                parada.Observacion = dto.Observacion;
                parada.FechaModificacion = DateTime.Now;
                parada.UsuarioModificacion = usuarioActual;

                var detallesViejos = _context.ParteParadaDetalles
                    .Where(d => d.NumeroParada.Trim() == numeroParada.Trim());
                _context.ParteParadaDetalles.RemoveRange(detallesViejos);
                await _context.SaveChangesAsync();

                int numeroLinea = 1;
                foreach (var det in dto.Detalles)
                {
                    _context.ParteParadaDetalles.Add(new ParteParadaDetalle
                    {
                        CodigoEmp = CODIGO_EMP_FIJO,
                        NumeroParada = parada.NumeroParada,
                        NumeroLinea = numeroLinea,
                        HoraInicial = det.HoraInicial,
                        HoraFinal = det.HoraFinal,
                        CodigoMotivo = det.CodigoMotivo,
                        Observacion = det.Observacion,
                        FechaCreacion = DateTime.Now,
                        UsuarioCreacion = usuarioActual
                    });
                    numeroLinea++;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { mensaje = "Parada actualizada correctamente" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { mensaje = "Error al actualizar", detalle = ex.Message });
            }
        }

        [HttpDelete("{numeroParada}")]
        [Authorize(Roles = ROL_ADMIN)]
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

        // Un Admin puede ver/editar cualquier parada;
        // un Supervisor, solo la suya (Supervisadopor == su CodigoPersonal);
        // un operador, solo la que él mismo creó.
        private bool TienePermisoSobre(ParteParada parada)
        {
            if (User.IsInRole(ROL_ADMIN))
                return true;

            if (User.IsInRole(ROL_SUPERVISOR))                                // ← nuevo
            {
                var codigoPersonalActual = User.FindFirstValue("CodigoPersonal");
                return codigoPersonalActual != null &&
                       parada.Supervisadopor != null &&
                       parada.Supervisadopor.Trim() == codigoPersonalActual;
            }

            var usuarioActual = User.FindFirstValue(ClaimTypes.Name);
            return parada.UsuarioCreacion != null && parada.UsuarioCreacion.Trim() == usuarioActual;
        }

        private async Task<string> GenerarSiguienteNumeroParada()
        {
            var numerosExistentes = await _context.ParteParadas
                .Select(p => p.NumeroParada)
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

            return siguiente.ToString("D10");
        }
    }
}