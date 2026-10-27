using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoIntegradorFinal.Data;
using ProyectoIntegradorFinal.Models;

namespace ProyectoIntegradorFinal.ApiControllers
{
    [Route("api/productos")]
    [ApiController]
    public class ProductosController : Controller
    {

        private readonly AppDbContext _context;

        public ProductosController(AppDbContext context)
        {
            _context = context;
        }


        
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Producto>>> GetProductos()
        {
            return await _context.Productos
                .Include(p => p.Marca)
                .Include(p => p.Categoria)
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Producto>> GetProducto(int id)
        {
            var producto = await _context.Productos
                .Include(p => p.Marca)
                .Include(p => p.Categoria)
                .FirstOrDefaultAsync(p => p.IdProducto == id);

            if (producto == null)
            {
                return NotFound(new { mensaje = $"No se encontró el producto con el ID {id}" });
            }

            return producto;
        }

        [HttpPost]
        public async Task<ActionResult<Producto>> PostProducto(Producto producto)
        {
            // Validar opcionalmente si la Marca y la Categoría existen
            var marcaExiste = await _context.Marcas.AnyAsync(m => m.IdMarca == producto.IdMarca);
            var categoriaExiste = await _context.Categorias.AnyAsync(c => c.IdCategoria == producto.IdCategoria);

            if (!marcaExiste || !categoriaExiste)
            {
                return BadRequest(new { mensaje = "La Marca o la Categoría especificada no existen." });
            }

            _context.Productos.Add(producto);
            await _context.SaveChangesAsync();

            // Recargar el producto con sus relaciones para devolverlo completo
            await _context.Entry(producto).Reference(p => p.Marca).LoadAsync();
            await _context.Entry(producto).Reference(p => p.Categoria).LoadAsync();

            return CreatedAtAction(nameof(GetProducto), new { id = producto.IdProducto }, producto);
        }

        // 4. PUT: api/productos/5 (Actualizar un producto existente)
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProducto(int id, Producto producto)
        {
            if (id != producto.IdProducto)
            {
                return BadRequest(new { mensaje = "El ID del producto no coincide con la ruta." });
            }

            _context.Entry(producto).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Productos.AnyAsync(e => e.IdProducto == id))
                {
                    return NotFound(new { mensaje = $"No se encontró el producto con el ID {id}" });
                }
                else
                {
                    throw;
                }
            }

            return NoContent(); 
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProducto(int id)
        {
            var producto = await _context.Productos.FindAsync(id);
            if (producto == null)
            {
                return NotFound(new { mensaje = $"No se encontró el producto con el ID {id}" });
            }

            _context.Productos.Remove(producto);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/productos/categoria/2
        [HttpGet("categoria/{idCategoria}")]
        public async Task<ActionResult<IEnumerable<Producto>>> GetProductosPorCategoria(int idCategoria)
        {
            return await _context.Productos
                .Where(p => p.IdCategoria == idCategoria)
                .Include(p => p.Marca)
                .Include(p => p.Categoria)
                .ToListAsync();
        }

        // GET: api/productos/marca/5
        [HttpGet("marca/{idMarca}")]
        public async Task<ActionResult<IEnumerable<Producto>>> GetProductosPorMarca(int idMarca)
        {
            return await _context.Productos
                .Where(p => p.IdMarca == idMarca)
                .Include(p => p.Marca)
                .Include(p => p.Categoria)
                .ToListAsync();
        }
    }
}
