using ApiTransporteLweb.Data;
using ApiTransporteLweb.Models;
using ApiTransporteLweb.Models.Dtos;
using ApiTransporteLweb.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Cryptography;

namespace ApiTransporteLweb.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        // Cuánto dura el link de recuperación antes de expirar
        private static readonly TimeSpan DURACION_TOKEN = TimeSpan.FromMinutes(30);

        // URL del front donde se pega el token para armar el link (ajusta al dominio real)
        private const string URL_BASE_FRONT = "http://localhost:5173";

        private readonly ApplicationDbContext _context;
        private readonly JwtService _jwtService;
        private readonly EmailService _emailService;

        public AuthController(ApplicationDbContext context, JwtService jwtService, EmailService emailService)
        {
            _context = context;
            _jwtService = jwtService;
            _emailService = emailService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto login)
        {
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.NombreUsuario == login.NombreUsuario && u.Activo);

            if (usuario == null || !BCrypt.Net.BCrypt.Verify(login.Password, usuario.Password))
                return Unauthorized(new { mensaje = "Usuario o contraseña incorrectos" });

            var token = _jwtService.GenerarToken(usuario);

            return Ok(new
            {
                mensaje = "Login exitoso",
                token,
                usuarioId = usuario.Id,
                nombreUsuario = usuario.NombreUsuario,
                nombre = usuario.Nombre,
                rol = usuario.Rol
            });
        }

        [HttpPost("registrar-usuario")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RegistrarUsuario([FromBody] RegistrarUsuarioDto dto)
        {
            var existe = await _context.Usuarios.AnyAsync(u => u.NombreUsuario == dto.NombreUsuario);
            if (existe)
                return BadRequest(new { mensaje = "Ese usuario ya existe" });

            var usuario = new Usuario
            {
                NombreUsuario = dto.NombreUsuario,
                Password = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Nombre = dto.Nombre,
                Email = dto.Email,
                Activo = true,
                FechaCreacion = DateTime.Now,
                Rol = dto.Rol ?? "Usuario"
            };

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Usuario creado correctamente", usuarioId = usuario.Id });
        }

        // GET /api/Auth/usuarios -> listar todos los usuarios (solo Admin)
        [HttpGet("usuarios")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ListarUsuarios()
        {
            var usuarios = await _context.Usuarios
                .OrderBy(u => u.NombreUsuario)
                .Select(u => new
                {
                    u.Id,
                    u.NombreUsuario,
                    u.Nombre,
                    u.Email,
                    u.Activo,
                    u.FechaCreacion,
                    u.Rol
                })
                .ToListAsync();

            return Ok(usuarios);
        }

        // PUT /api/Auth/usuarios/{id}/activar -> solo Admin
        [HttpPut("usuarios/{id}/activar")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ActivarUsuario(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
                return NotFound(new { mensaje = "Usuario no encontrado" });

            usuario.Activo = true;
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Usuario activado correctamente" });
        }

        // PUT /api/Auth/usuarios/{id}/desactivar -> solo Admin
        [HttpPut("usuarios/{id}/desactivar")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DesactivarUsuario(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
                return NotFound(new { mensaje = "Usuario no encontrado" });

            var usuarioActualId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (usuarioActualId != null && usuarioActualId == usuario.Id.ToString())
                return BadRequest(new { mensaje = "No puedes desactivar tu propia cuenta" });

            usuario.Activo = false;
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Usuario desactivado correctamente" });
        }

        // DELETE /api/Auth/usuarios/{id} -> solo Admin
        [HttpDelete("usuarios/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> EliminarUsuario(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
                return NotFound(new { mensaje = "Usuario no encontrado" });

            var usuarioActualId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (usuarioActualId != null && usuarioActualId == usuario.Id.ToString())
                return BadRequest(new { mensaje = "No puedes eliminar tu propia cuenta" });

            _context.Usuarios.Remove(usuario);
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Usuario eliminado correctamente" });
        }

        // POST /api/Auth/solicitar-recuperacion -> genera el token y manda el correo
        [HttpPost("solicitar-recuperacion")]
        public async Task<IActionResult> SolicitarRecuperacion([FromBody] SolicitarRecuperacionDto dto)
        {
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Email != null && u.Email == dto.Email && u.Activo);

            // Siempre se responde lo mismo exista o no el correo, para no revelar
            // qué correos están registrados (evita que cualquiera "descubra" usuarios).
            var mensajeGenerico = new
            {
                mensaje = "Si el correo está registrado, te llegará un enlace para restablecer tu contraseña."
            };

            if (usuario == null)
                return Ok(mensajeGenerico);

            var tokenBytes = RandomNumberGenerator.GetBytes(32);
            var token = Convert.ToBase64String(tokenBytes)
                .Replace("+", "-").Replace("/", "_").Replace("=", "");

            usuario.TokenRecuperacion = token;
            usuario.TokenExpiracion = DateTime.Now.Add(DURACION_TOKEN);
            await _context.SaveChangesAsync();

            var link = $"{URL_BASE_FRONT}/restablecer-password?token={token}";
            var cuerpo = $@"
                <p>Hola {usuario.Nombre ?? usuario.NombreUsuario},</p>
                <p>Pediste restablecer tu contraseña en Transportes Luchito. Este enlace vence en 30 minutos:</p>
                <p><a href=""{link}"">Restablecer contraseña</a></p>
                <p>Si no fuiste tú, ignora este correo.</p>";

            await _emailService.EnviarCorreoAsync(usuario.Email!, "Recuperar contraseña", cuerpo);

            return Ok(mensajeGenerico);
        }

        // POST /api/Auth/restablecer-password -> valida el token y guarda la nueva contraseña
        [HttpPost("restablecer-password")]
        public async Task<IActionResult> RestablecerPassword([FromBody] RestablecerPasswordDto dto)
        {
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.TokenRecuperacion == dto.Token);

            if (usuario == null || usuario.TokenExpiracion == null || usuario.TokenExpiracion < DateTime.Now)
                return BadRequest(new { mensaje = "El enlace no es válido o ya expiró" });

            usuario.Password = BCrypt.Net.BCrypt.HashPassword(dto.NuevaPassword);
            usuario.TokenRecuperacion = null;
            usuario.TokenExpiracion = null;
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Contraseña actualizada correctamente" });
        }
    }
}