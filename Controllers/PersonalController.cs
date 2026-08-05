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
    }
}