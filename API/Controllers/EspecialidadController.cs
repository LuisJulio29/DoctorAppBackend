﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Data;
using Models.Entities;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EspecialidadController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public EspecialidadController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Especialidad
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Especialidad>>> GetEspecialidades()
        {
            return await _context.Especialidades.ToListAsync();
        }

        // GET: api/Especialidad/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Especialidad>> GetEspecialidad(int id)
        {
            var especialidad = await _context.Especialidades.FindAsync(id);

            if (especialidad == null)
            {
                return NotFound();
            }

            return especialidad;
        }

        // PUT: api/Especialidad/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutEspecialidad(int id, Especialidad especialidad)
        {
            if (id != especialidad.Id)
            {
                return BadRequest();
            }

            _context.Entry(especialidad).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EspecialidadExists(id))
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

        // POST: api/Especialidad
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Especialidad>> PostEspecialidad(Especialidad especialidad)
        {
            _context.Especialidades.Add(especialidad);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetEspecialidad", new { id = especialidad.Id }, especialidad);
        }

        // DELETE: api/Especialidad/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEspecialidad(int id)
        {
            var especialidad = await _context.Especialidades.FindAsync(id);
            if (especialidad == null)
            {
                return NotFound();
            }

            _context.Especialidades.Remove(especialidad);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool EspecialidadExists(int id)
        {
            return _context.Especialidades.Any(e => e.Id == id);
        }
    }
}
