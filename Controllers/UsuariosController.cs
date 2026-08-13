using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ApiTransporteLweb.Data;
using ApiTransporteLweb.Models.Dtos;

namespace ApiTransporteLweb.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsuariosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UsuariosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Solo el admin puede listar todos los usuarios
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ObtenerUsuarios()
        {
            var usuarios = await _context.Usuarios
                .Select(u => new
                {
                    u.Id,
                    u.NombreUsuario,
                    u.Nombre,
                    u.Email,
                    u.Rol,
                    u.Activo,
                    u.FechaCreacion
                })
                .ToListAsync();

            return Ok(usuarios);
        }

        // Admin puede cambiar la contraseña de cualquiera.
        // Un usuario normal solo puede cambiar la suya propia, y debe confirmar su contraseña actual.
        [HttpPut("{id}/password")]
        public async Task<IActionResult> CambiarPassword(int id, [FromBody] CambiarPasswordDto dto)
        {
            var esAdmin = User.IsInRole("Admin");
            var idActual = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            if (!esAdmin && idActual != id)
                return Forbid();

            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
                return NotFound(new { mensaje = "Usuario no encontrado" });

            // Si NO es admin, debe confirmar su contraseña actual antes de cambiarla
            if (!esAdmin)
            {
                if (string.IsNullOrEmpty(dto.PasswordActual) ||
                    !BCrypt.Net.BCrypt.Verify(dto.PasswordActual, usuario.Password))
                {
                    return BadRequest(new { mensaje = "La contraseña actual no es correcta" });
                }
            }

            usuario.Password = BCrypt.Net.BCrypt.HashPassword(dto.NuevaPassword);
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Contraseña actualizada correctamente" });
        }
        [HttpPut("por-nombre/{nombreUsuario}/password")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CambiarPasswordPorNombre(string nombreUsuario, [FromBody] CambiarPasswordDto dto)
        {
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.NombreUsuario == nombreUsuario);

            if (usuario == null)
                return NotFound(new { mensaje = "Usuario no encontrado" });

            usuario.Password = BCrypt.Net.BCrypt.HashPassword(dto.NuevaPassword);
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Contraseña actualizada correctamente", nombreUsuario = usuario.NombreUsuario });
        }
    }
}