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

        public UsuarioController(ApplicationDbContext context,ITokenService tokenService)
        {
            _context = context;
            _tokenService = tokenService;
        }

        // GET: api/Usuario
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Usuario>>> GetUsarios()
        {
            return await _context.Usarios.ToListAsync();
        }

        // GET: api/Usuario/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Usuario>> GetUsuario(int id)
        {
            var usuario = await _context.Usarios.FindAsync(id);

            if (usuario == null)
            {
                return NotFound();
            }

            return usuario;
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

        // POST: api/Usuario/Register
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost("Register")]
        public async Task<ActionResult<UserDTO>> PostUsuario(RegisterDTO registerDTO)
        {
            if(await UsuarioExists(registerDTO.UserName))
            {
                return BadRequest("UserName already exists");
            }
            using var hmac = new System.Security.Cryptography.HMACSHA512();
            var usuario = new Usuario
            {
                UserName = registerDTO.UserName.ToLower(),
                PasswordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(registerDTO.Password)),
                PasswordSalt = hmac.Key
            };
            _context.Usarios.Add(usuario);
            await _context.SaveChangesAsync();
            return new UserDTO
            {
                UserName = usuario.UserName,
                Token = _tokenService.CreateToken(usuario)
            };
        }

        [HttpPost("Login")]
        public async Task<ActionResult<UserDTO>> Login(LoginDTO loginDTO)
        {
            var usuario = await _context.Usarios.SingleOrDefaultAsync(x => x.UserName == loginDTO.UserName);
            if(usuario == null)
            {
                return Unauthorized("Invalid UserName");
            }
            using var hmac = new System.Security.Cryptography.HMACSHA512(usuario.PasswordSalt);
            var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(loginDTO.Password));
            for (int i = 0; i < computedHash.Length; i++)
            {
                if (computedHash[i] != usuario.PasswordHash[i])
                {
                    return Unauthorized("Invalid Password");
                }
            }
            return new ActionResult<UserDTO>(new UserDTO
            {
                UserName = usuario.UserName,
                Token = _tokenService.CreateToken(usuario)
            });
        }
        // DELETE: api/Usuario/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUsuario(int id)
        {
            var usuario = await _context.Usarios.FindAsync(id);
            if (usuario == null)
            {
                return NotFound();
            }

            _context.Usarios.Remove(usuario);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UsuarioExists(int id)
        {
            return _context.Usarios.Any(e => e.Id == id);
        }
        private async Task<bool> UsuarioExists(string username)
        {
            return await _context.Usarios.AnyAsync(e => e.UserName == username.ToLower());
        }
    }
}
