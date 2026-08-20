using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ApiTransporteLweb.Data;

namespace ApiTransporteLweb.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MotivosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public MotivosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET /api/Motivos -> para el select de Motivo en el detalle de Parte de Parada
        [HttpGet]
        public async Task<IActionResult> ObtenerMotivos()
        {
            var motivos = await _context.VwOperacionesMotivos
                .OrderBy(m => m.Motivo)
                .Select(m => new
                {
                    codigo = m.Codigo.Trim(),
                    motivo = m.Motivo.Trim()
                })
                .ToListAsync();

            return Ok(motivos);
        }
    }
}