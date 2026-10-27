using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoIntegradorFinal.Data;
using ProyectoIntegradorFinal.Models;

namespace ProyectoIntegradorFinal.ApiControllers
{
    [Route("api/marca")]
    [ApiController]
    public class MarcaController : Controller
    {
        private readonly AppDbContext _context;

        public MarcaController(AppDbContext context)
        {
            _context = context;
        }


        [HttpGet]
        public async Task<ActionResult<IEnumerable<Marca>>> GetMarcas()
        {
            return await _context.Marcas.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Marca>> GetMarca(int id)
        {
            var marca = await _context.Marcas
                .Include(m => m.Productos) // Trae los perfumes de esta marca si lo deseas
                .FirstOrDefaultAsync(m => m.IdMarca == id);

            if (marca == null)
            {
                return NotFound(new { mensaje = $"No se encontró la marca con el ID {id}" });
            }

            return marca;
        }


        // 3. POST: api/marcas (Crear una nueva marca)
        [HttpPost]
        public async Task<ActionResult<Marca>> PostMarca(Marca marca)
        {
            _context.Marcas.Add(marca);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetMarca), new { id = marca.IdMarca }, marca);
        }


        // 4. PUT: api/marcas/5 (Actualizar una marca)
        [HttpPut("{id}")]
        public async Task<IActionResult> PutMarca(int id, Marca marca)
        {
            if (id != marca.IdMarca)
            {
                return BadRequest(new { mensaje = "El ID de la marca no coincide con la ruta." });
            }

            _context.Entry(marca).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Marcas.AnyAsync(e => e.IdMarca == id))
                {
                    return NotFound(new { mensaje = $"No se encontró la marca con el ID {id}" });
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }


        // 5. DELETE: api/marcas/5 (Eliminar una marca)
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMarca(int id)
        {
            var marca = await _context.Marcas.FindAsync(id);
            if (marca == null)
            {
                return NotFound(new { mensaje = $"No se encontró la marca con el ID {id}" });
            }

            // Opcional: Validar si tiene productos asociados antes de eliminar
            var tieneProductos = await _context.Productos.AnyAsync(p => p.IdMarca == id);
            if (tieneProductos)
            {
                return BadRequest(new { mensaje = "No se puede eliminar la marca porque tiene productos asociados." });
            }

            _context.Marcas.Remove(marca);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
