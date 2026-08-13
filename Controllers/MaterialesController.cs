using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ApiTransporteLweb.Data;

namespace ApiTransporteLweb.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MaterialesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public MaterialesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET /api/Materiales
        [HttpGet]
        public async Task<IActionResult> ObtenerMateriales()
        {
            var materiales = await _context.VwOperacionesMateriales
                .OrderBy(m => m.Nombrem)
                .Select(m => new
                {
                    codigoM = m.Codigom.Trim(),
                    nombreM = m.Nombrem.Trim(),
                    abreviatura = m.Abreviatura.Trim()
                })
                .ToListAsync();

            return Ok(materiales);
        }
    }
}