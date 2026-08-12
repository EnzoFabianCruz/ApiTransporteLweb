using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ApiTransporteLweb.Data;

namespace ApiTransporteLweb.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CiclosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CiclosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET /api/Ciclos -> para el select de Ciclo en el detalle del Parte de Trabajo.
        // Al elegir un ciclo (mostrando "valor"), el frontend autocompleta
        // CodigoOrigen, CodigoDestino y ValorCiclo, y puede mostrar nombreOrigen/nombreDestino.
        [HttpGet]
        public async Task<IActionResult> ObtenerCiclos()
        {
            var ciclos = await (
                from ciclo in _context.UnidadesCiclos
                join origen in _context.CiclosPuntos
                    on ciclo.CodigoOrigen equals origen.CodigoPunto into origenJoin
                from origen in origenJoin.DefaultIfEmpty()
                join destino in _context.CiclosPuntos
                    on ciclo.CodigoDestino equals destino.CodigoPunto into destinoJoin
                from destino in destinoJoin.DefaultIfEmpty()
                orderby ciclo.Valor
                select new
                {
                    codigoCiclo = ciclo.CodigoCiclo,
                    valor = ciclo.Valor,
                    codigoOrigen = ciclo.CodigoOrigen,
                    nombreOrigen = origen != null ? origen.NombrePunto : null,
                    codigoDestino = ciclo.CodigoDestino,
                    nombreDestino = destino != null ? destino.NombrePunto : null
                }
            ).ToListAsync();

            return Ok(ciclos);
        }
    }
}