using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ApiTransporteLweb.Data;
using ApiTransporteLweb.Models;
using ApiTransporteLweb.Models.Dtos;
using ApiTransporteLweb.Services;

namespace ApiTransporteLweb.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly JwtService _jwtService;

        public AuthController(ApplicationDbContext context, JwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
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
                Activo = true,
                FechaCreacion = DateTime.Now,
                Rol = dto.Rol ?? "Usuario"
            };

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Usuario creado correctamente", usuarioId = usuario.Id });
        }
    }
}