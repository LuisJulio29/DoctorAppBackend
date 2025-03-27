using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Data;
using Models.Entities;
using Models.DTOs;
using Data.Interfaces;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuarioController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ITokenService _tokenService;

        public UsuarioController(ApplicationDbContext context, ITokenService tokenService)
        {
            _context = context;
            _tokenService = tokenService;
        }

        // GET: api/Usuario
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Usuario>>> GetUsarios()
        {
            return await _context.Usuarios.ToListAsync();
        }

        // GET: api/Usuario/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Usuario>> GetUsuario(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);

            if (usuario == null)
            {
                return NotFound();
            }

            return usuario;
        }
        // POST: api/Usuario
        [HttpPost]
        public async Task<ActionResult<Usuario>> PostUsuario(RegisterDTO usuario)
        {
            if (await UsuarioExists(usuario.UserName))
            {
                return BadRequest("El usuario ya existe");
            }
            var user = new Usuario
            {
                UserName = usuario.UserName
            };
            _context.Usuarios.Add(user);
            await _context.SaveChangesAsync();
            return CreatedAtAction("GetUsuario", new { id = user.Id }, user);
        }

        [HttpPost("Register")]
        public async Task<ActionResult<UserDTO>> Register(RegisterDTO usuario)
        {
            if (await UsuarioExists(usuario.UserName))
            {
                return BadRequest("El usuario ya existe");
            }
            using var hmac = new System.Security.Cryptography.HMACSHA512();
            var user = new Usuario
            {
                UserName = usuario.UserName,
                PasswordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(usuario.Password)),
                PasswordSalt = hmac.Key
            };
            _context.Usuarios.Add(user);
            await _context.SaveChangesAsync();
            return new ActionResult<UserDTO>(new UserDTO
            {
                UserName = user.UserName,
                Token = _tokenService.CreateToken(user)
            });
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDTO>> login(LoginDTO usuario)
        {
            var user = await _context.Usuarios.SingleOrDefaultAsync(x => x.UserName == usuario.UserName);
            if (user == null) return Unauthorized("Usuario no existe");
            using var hmac = new System.Security.Cryptography.HMACSHA512(user.PasswordSalt);
            var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(usuario.Password));
            for (int i = 0; i < computedHash.Length; i++)
            {
                if (computedHash[i] != user.PasswordHash[i]) return Unauthorized("Contraseña incorrecta");
            }
            return new ActionResult<UserDTO>(new UserDTO {
                UserName = user.UserName,
                Token = _tokenService.CreateToken(user)
            });
        }

        // PUT: api/Usuario/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUsuario(int id, Usuario usuario)
        {
            if (id != usuario.Id)
            {
                return BadRequest();
            }

            _context.Entry(usuario).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UsuarioExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }
        // DELETE: api/Usuario/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUsuario(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
            {
                return NotFound();
            }

            _context.Usuarios.Remove(usuario);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UsuarioExists(int id)
        {
            return _context.Usuarios.Any(e => e.Id == id);
        }
        private async Task<bool> UsuarioExists(string username)
        {
            return await _context.Usuarios.AnyAsync(e => e.UserName == username);
        }
    }
}
