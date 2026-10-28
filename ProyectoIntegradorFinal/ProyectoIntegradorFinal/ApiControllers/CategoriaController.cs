using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProyectoIntegradorFinal.Data;
using ProyectoIntegradorFinal.Models;
using Microsoft.EntityFrameworkCore;


namespace ProyectoIntegradorFinal.ApiControllers
{
    [Route("api/categoria")]
    [ApiController]
    public class CategoriaController : Controller
    {
        private readonly AppDbContext _context;

        public CategoriaController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Categoria>>> GetCategorias()
        {
            return await _context.Categorias.ToListAsync();
            
        }


        // GET: api/categoria/buscar?termino=flor
        [HttpGet("buscar")]
        public async Task<IActionResult> BuscarCategorias([FromQuery] string termino)
        {
            if (string.IsNullOrWhiteSpace(termino))
            {
                return Ok(Array.Empty<object>());
            }

            termino = termino.Trim();

            var categorias = await _context.Categorias
                .AsNoTracking()
                .Where(c => c.Nombre.Contains(termino))
                .OrderBy(c => c.Nombre)
                .Take(10)
                .Select(c => new
                {
                    idCategoria = c.IdCategoria,
                    nombre = c.Nombre
                })
                .ToListAsync();

            return Ok(categorias);
        }


        [HttpGet("{id}")]
        public async Task<ActionResult<Categoria>> GetCategoria(int id)
        {
            var categoria = await _context.Categorias
                .Include(c => c.Productos)
                .FirstOrDefaultAsync(c => c.IdCategoria == id);

            if (categoria == null)
            {
                return NotFound(new { mensaje = $"No se encontró la categoría con el ID {id}" });
            }

            return categoria;
        }

        [HttpPost]
        public async Task<ActionResult<Categoria>> PostCategoria(Categoria categoria)
        {
            _context.Categorias.Add(categoria);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCategoria), new { id = categoria.IdCategoria }, categoria);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutCategoria(int id, Categoria categoria)
        {
            if (id != categoria.IdCategoria)
            {
                return BadRequest(new { mensaje = "El ID de la categoría no coincide con la ruta." });
            }

            _context.Entry(categoria).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Categorias.AnyAsync(e => e.IdCategoria == id))
                {
                    return NotFound(new { mensaje = $"No se encontró la categoría con el ID {id}" });
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategoria(int id)
        {
            var categoria = await _context.Categorias.FindAsync(id);
            if (categoria == null)
            {
                return NotFound(new { mensaje = $"No se encontró la categoría con el ID {id}" });
            }

            // Validar si tiene productos asociados antes de eliminar
            var tieneProductos = await _context.Productos.AnyAsync(p => p.IdCategoria == id);
            if (tieneProductos)
            {
                return BadRequest(new { mensaje = "No se puede eliminar la categoría porque tiene productos asociados." });
            }

            _context.Categorias.Remove(categoria);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
