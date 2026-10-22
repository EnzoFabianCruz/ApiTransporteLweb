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
        // GET /api/Usuarios/mi-perfil -> el usuario logueado ve sus propios datos
        [HttpGet("mi-perfil")]
        public async Task<IActionResult> ObtenerMiPerfil()
        {
            var idActual = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var usuario = await _context.Usuarios.FindAsync(idActual);
            if (usuario == null)
                return NotFound(new { mensaje = "Usuario no encontrado" });

            return Ok(new
            {
                usuario.Id,
                usuario.NombreUsuario,
                usuario.Nombre,
                usuario.Email,
                usuario.Rol,
                usuario.CodigoPersonal
            });
        }

        // PUT /api/Usuarios/mi-perfil -> el usuario logueado edita su Nombre y Email
        // (Rol y CodigoPersonal quedan fuera a propósito: eso solo lo toca el Admin)
        [HttpPut("mi-perfil")]
        public async Task<IActionResult> ActualizarMiPerfil([FromBody] ActualizarPerfilDto dto)
        {
            var idActual = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var usuario = await _context.Usuarios.FindAsync(idActual);
            if (usuario == null)
                return NotFound(new { mensaje = "Usuario no encontrado" });

            if (!string.IsNullOrWhiteSpace(dto.Email) && dto.Email != usuario.Email)
            {
                var emailEnUso = await _context.Usuarios.AnyAsync(u => u.Id != idActual && u.Email == dto.Email);
                if (emailEnUso)
                    return BadRequest(new { mensaje = "Ese correo ya está en uso por otro usuario" });
            }

            usuario.Nombre = dto.Nombre;
            usuario.Email = dto.Email;
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Perfil actualizado correctamente" });
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
        // PUT /api/Usuarios/{id} -> Admin edita los datos/credenciales de cualquier usuario
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ActualizarUsuario(int id, [FromBody] ActualizarUsuarioAdminDto dto)
        {
            var idActual = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
                return NotFound(new { mensaje = "Usuario no encontrado" });

            if (string.IsNullOrWhiteSpace(dto.NombreUsuario))
                return BadRequest(new { mensaje = "El nombre de usuario es obligatorio" });

            var rolesValidos = new[] { "Usuario", "Supervisor", "Admin" };
            if (!rolesValidos.Contains(dto.Rol))
                return BadRequest(new { mensaje = "Rol no válido" });

            if (!string.IsNullOrEmpty(dto.NuevaPassword) && dto.NuevaPassword.Length < 6)
                return BadRequest(new { mensaje = "La contraseña debe tener al menos 6 caracteres" });

            // Evita que el admin se quite a sí mismo el acceso por error
            if (id == idActual && (dto.Rol != "Admin" || !dto.Activo))
                return BadRequest(new { mensaje = "No puedes quitarte el rol Admin ni desactivarte a ti mismo" });

            // Nombre de usuario único
            if (dto.NombreUsuario != usuario.NombreUsuario)
            {
                var existe = await _context.Usuarios
                    .AnyAsync(u => u.Id != id && u.NombreUsuario == dto.NombreUsuario);
                if (existe)
                    return BadRequest(new { mensaje = "Ese nombre de usuario ya está en uso" });
            }

            // Email único (solo si viene con valor)
            if (!string.IsNullOrWhiteSpace(dto.Email) && dto.Email != usuario.Email)
            {
                var emailEnUso = await _context.Usuarios
                    .AnyAsync(u => u.Id != id && u.Email == dto.Email);
                if (emailEnUso)
                    return BadRequest(new { mensaje = "Ese correo ya está en uso por otro usuario" });
            }

            usuario.NombreUsuario = dto.NombreUsuario;
            usuario.Nombre = dto.Nombre;
            usuario.Email = dto.Email;
            usuario.Rol = dto.Rol;
            usuario.CodigoPersonal = dto.CodigoPersonal;
            usuario.Activo = dto.Activo;

            if (!string.IsNullOrWhiteSpace(dto.NuevaPassword))
                usuario.Password = BCrypt.Net.BCrypt.HashPassword(dto.NuevaPassword);

            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Usuario actualizado correctamente" });
        }
    }
}