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
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<Producto>> PostProducto(
    [FromForm] Producto producto,
    [FromForm] IFormFile? archivoImagen)
        {
            var marcaExiste = await _context.Marcas
                .AnyAsync(m => m.IdMarca == producto.IdMarca);

            var categoriaExiste = await _context.Categorias
                .AnyAsync(c => c.IdCategoria == producto.IdCategoria);

            if (!marcaExiste || !categoriaExiste)
            {
                return BadRequest(new
                {
                    mensaje = "La Marca o la Categoría especificada no existen."
                });
            }

            if (archivoImagen != null && archivoImagen.Length > 0)
            {
                var extensionesPermitidas = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                var extension = Path.GetExtension(archivoImagen.FileName).ToLowerInvariant();

                if (!extensionesPermitidas.Contains(extension))
                {
                    return BadRequest(new
                    {
                        mensaje = "La imagen debe ser JPG, JPEG, PNG o WEBP."
                    });
                }

                if (archivoImagen.Length > 2 * 1024 * 1024)
                {
                    return BadRequest(new
                    {
                        mensaje = "La imagen no puede superar los 2 MB."
                    });
                }

                var carpeta = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot",
                    "images",
                    "productos");

                Directory.CreateDirectory(carpeta);

                var nombreArchivo = $"{Guid.NewGuid()}{extension}";
                var rutaArchivo = Path.Combine(carpeta, nombreArchivo);

                using (var stream = new FileStream(rutaArchivo, FileMode.Create))
                {
                    await archivoImagen.CopyToAsync(stream);
                }

                producto.ImagenUrl = $"/images/productos/{nombreArchivo}";
            }

            _context.Productos.Add(producto);
            await _context.SaveChangesAsync();

            await _context.Entry(producto).Reference(p => p.Marca).LoadAsync();
            await _context.Entry(producto).Reference(p => p.Categoria).LoadAsync();

            return CreatedAtAction(
                nameof(GetProducto),
                new { id = producto.IdProducto },
                producto);
        }

        // 4. PUT: api/productos/5 (Actualizar un producto existente)

        [HttpPut("{id}")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> PutProducto(
            int id,
            [FromForm] Producto datos,
            [FromForm] IFormFile? archivoImagen)
        {
            if (id != datos.IdProducto)
            {
                return BadRequest(new
                {
                    mensaje = "El ID del producto no coincide con la ruta."
                });
            }

            var producto = await _context.Productos.FindAsync(id);

            if (producto == null)
            {
                return NotFound(new
                {
                    mensaje = $"No se encontró el producto con el ID {id}"
                });
            }

            var marcaExiste = await _context.Marcas
                .AnyAsync(m => m.IdMarca == datos.IdMarca);

            var categoriaExiste = await _context.Categorias
                .AnyAsync(c => c.IdCategoria == datos.IdCategoria);

            if (!marcaExiste || !categoriaExiste)
            {
                return BadRequest(new
                {
                    mensaje = "La Marca o la Categoría especificada no existen."
                });
            }

            producto.Nombre = datos.Nombre;
            producto.Descripcion = datos.Descripcion;
            producto.Precio = datos.Precio;
            producto.Stock = datos.Stock;
            producto.VolumenMl = datos.VolumenMl;
            producto.Concentracion = datos.Concentracion;
            producto.IdMarca = datos.IdMarca;
            producto.IdCategoria = datos.IdCategoria;

            // Si no se selecciona otra imagen, se conserva la actual.
            if (archivoImagen != null && archivoImagen.Length > 0)
            {
                var extensionesPermitidas = new[]
                {
            ".jpg", ".jpeg", ".png", ".webp"
        };

                var extension = Path.GetExtension(archivoImagen.FileName)
                    .ToLowerInvariant();

                if (!extensionesPermitidas.Contains(extension))
                {
                    return BadRequest(new
                    {
                        mensaje = "La imagen debe ser JPG, JPEG, PNG o WEBP."
                    });
                }

                if (archivoImagen.Length > 2 * 1024 * 1024)
                {
                    return BadRequest(new
                    {
                        mensaje = "La imagen no puede superar los 2 MB."
                    });
                }

                var carpeta = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot",
                    "images",
                    "productos");

                Directory.CreateDirectory(carpeta);

                var nombreArchivo = $"{Guid.NewGuid()}{extension}";
                var rutaArchivo = Path.Combine(carpeta, nombreArchivo);

                using (var stream = new FileStream(rutaArchivo, FileMode.Create))
                {
                    await archivoImagen.CopyToAsync(stream);
                }

                producto.ImagenUrl = $"/images/productos/{nombreArchivo}";
            }

            await _context.SaveChangesAsync();

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
