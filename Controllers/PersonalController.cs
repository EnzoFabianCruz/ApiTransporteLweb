using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ApiTransporteLweb.Data;

namespace ApiTransporteLweb.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PersonalController : ControllerBase
    {
        private const string CODIGO_PUESTO_OPERADOR = "021";
        private const string CODIGO_PUESTO_SUPERVISOR = "027";

        private readonly ApplicationDbContext _context;

        public PersonalController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerPersonal()
        {
            var personal = await _context.Personal
                .OrderBy(p => p.NombreCompleto)
                .Select(p => new
                {
                    codigoPersonal = p.CodigoPersonal,
                    nombreCompleto = p.NombreCompleto
                })
                .ToListAsync();

            return Ok(personal);
        }

        // GET /api/Personal/operadores -> para el select de Operador (CodigoAnalitico)
        [HttpGet("operadores")]
        public async Task<IActionResult> ObtenerOperadores()
        {
            var operadores = await _context.Personal
                .Where(p => p.PreCargo == CODIGO_PUESTO_OPERADOR)
                .OrderBy(p => p.NombreCompleto)
                .Select(p => new
                {
                    codigoPersonal = p.CodigoPersonal,
                    nombreCompleto = p.NombreCompleto
                })
                .ToListAsync();

            return Ok(operadores);
        }

        // GET /api/Personal/supervisores -> para el select de Supervisado
        [HttpGet("supervisores")]
        public async Task<IActionResult> ObtenerSupervisores()
        {
            var supervisores = await _context.Personal
                .Where(p => p.PreCargo == CODIGO_PUESTO_SUPERVISOR)
                .OrderBy(p => p.NombreCompleto)
                .Select(p => new
                {
                    codigoPersonal = p.CodigoPersonal,
                    nombreCompleto = p.NombreCompleto
                })
                .ToListAsync();

            return Ok(supervisores);
        }
    }
}