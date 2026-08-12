using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ApiTransporteLweb.Data;

namespace ApiTransporteLweb.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UnidadesTransporteController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UnidadesTransporteController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET /api/UnidadesTransporte -> para el select de Unidad (CodigoUnidad)
        [HttpGet]
        public async Task<IActionResult> ObtenerUnidades()
        {
            var unidades = await _context.UnidadesTransporte
                .OrderBy(u => u.CodigoUnidad)
                .Select(u => new
                {
                    codigoUnidad = u.CodigoUnidad,
                    volq = u.Volq,
                    placa = u.Placa
                })
                .ToListAsync();

            return Ok(unidades);
        }
    }
}