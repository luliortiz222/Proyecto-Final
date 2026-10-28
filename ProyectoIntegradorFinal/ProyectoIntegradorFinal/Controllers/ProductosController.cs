using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;    
using ProyectoIntegradorFinal.Data;
using ProyectoIntegradorFinal.Models;
using Microsoft.AspNetCore.Authorization;

namespace ProyectoIntegradorFinal.Controllers
{
    public class ProductosController : Controller
    {
        private readonly AppDbContext _context;
        public ProductosController(AppDbContext context)
        {
            _context = context;
        }

        //GET prdouctos
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Index(string? buscar,int? idCategoria,int? idMarca, int? pagina)
        {

            int pageSize = 4;

            int pageNumber = pagina ?? 1;



            var consulta = _context.Productos
                .Include(p => p.Marca)
                .Include(p => p.Categoria)
                .AsQueryable();



            // Buscar en los productos guardados en MySQL
            if (!string.IsNullOrWhiteSpace(buscar))
            {
                buscar = buscar.Trim();
                string termino = $"%{buscar}%";

                consulta = consulta.Where(p =>
                    EF.Functions.Like(p.Nombre, termino) ||
                    (p.Marca != null &&
                     EF.Functions.Like(p.Marca.Nombre, termino))
                );
            }

            // Filtrar por categoría
            if (idCategoria.HasValue)
            {
                consulta = consulta.Where(p =>
                    p.IdCategoria == idCategoria.Value);
            }

            // Filtrar por marca
            if (idMarca.HasValue)
            {
                consulta = consulta.Where(p =>
                    p.IdMarca == idMarca.Value);
            }


            int totalRegistros = await consulta.CountAsync();


            var productos = await consulta
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .OrderBy(p => p.Nombre)
                .ToListAsync();

            ViewBag.Buscar = buscar;
            ViewBag.IdCategoria = idCategoria;
            ViewBag.IdMarca = idMarca;
            ViewBag.PaginaActual = pageNumber;
            ViewBag.TotalPaginas = (int)Math.Ceiling((double)totalRegistros / pageSize);

            ViewBag.Categorias = await _context.Categorias
                .OrderBy(c => c.Nombre)
                .ToListAsync();

            ViewBag.Marcas = await _context.Marcas
                .OrderBy(m => m.Nombre)
                .ToListAsync();

            return View(productos);
        }

        //GET : Productos/Create
        [Authorize(Roles = "Administrador")]
        public IActionResult Create() 
        {
            ViewData["IdMarca"] = new SelectList(_context.Marcas, "IdMarca", "Nombre");
            ViewData["IdCategoria"] = new SelectList(_context.Categorias, "IdCategoria", "Nombre");
            return View();
        }

        [Authorize(Roles = "Administrador")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Nombre,Descripcion,Precio,Stock,VolumenMl,Concentracion,IdMarca,IdCategoria")] Producto producto, IFormFile archivoImagen) 
        {
            if (ModelState.IsValid)
            {
                if (archivoImagen != null && archivoImagen.Length > 0)
                {
                    string nombreArchivo = Guid.NewGuid().ToString() + Path.GetExtension(archivoImagen.FileName);

                    string rutaCarpeta = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "perfumes");
                    if (!Directory.Exists(rutaCarpeta))
                    {
                        Directory.CreateDirectory(rutaCarpeta);
                    }

                    string rutaCompleta = Path.Combine(rutaCarpeta, nombreArchivo);

                    using (var stream = new FileStream(rutaCompleta, FileMode.Create))
                    {
                        await archivoImagen.CopyToAsync(stream);
                    }

                    producto.ImagenUrl = "/images/perfumes/" + nombreArchivo;
                }
                
                _context.Add(producto);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            else
            {
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    System.Diagnostics.Debug.WriteLine($"❌ ERROR DE MODELO: {error.ErrorMessage}");
                }
            }

            ViewData["IdMarca"] = new SelectList(_context.Marcas, "IdMarca", "Nombre", producto.IdMarca);
            ViewData["IdCategoria"] = new SelectList(_context.Categorias, "IdCategoria", "Nombre", producto.IdCategoria);

            return View(producto);
        }


        //GET : Productos/Edit/5
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Edit(int id) 
        {
            if (id == 0)
            {
                return NotFound();
            }

            var producto = await _context.Productos.FindAsync(id);
            if (producto == null)
            {
                return NotFound();
            }

            ViewData["IdMarca"] = new SelectList(_context.Marcas, "IdMarca", "Nombre", producto.IdMarca);
            ViewData["IdCategoria"] = new SelectList(_context.Categorias, "IdCategoria", "Nombre", producto.IdCategoria);

            return View(producto);
        }
        
        [Authorize(Roles = "Administrador")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdProducto,Nombre,Descripcion,Precio,Stock,VolumenMl,Concentracion,IdMarca,IdCategoria")] Producto producto, IFormFile? archivoImagen)
        {
            if (id != producto.IdProducto)
            {
                return NotFound();
            }

            // Recuperamos el producto original, incluida su imagen actual.
            var productoExistente = await _context.Productos.FindAsync(id);

            if (productoExistente == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Actualizamos los datos del producto.
                    productoExistente.Nombre = producto.Nombre;
                    productoExistente.Descripcion = producto.Descripcion;
                    productoExistente.Precio = producto.Precio;
                    productoExistente.Stock = producto.Stock;
                    productoExistente.VolumenMl = producto.VolumenMl;
                    productoExistente.Concentracion = producto.Concentracion;
                    productoExistente.IdMarca = producto.IdMarca;
                    productoExistente.IdCategoria = producto.IdCategoria;

                    // Solo reemplazamos la imagen si se seleccionó una nueva.
                    if (archivoImagen != null && archivoImagen.Length > 0)
                    {
                        string nombreArchivo = Guid.NewGuid().ToString()
                            + Path.GetExtension(archivoImagen.FileName);

                        string rutaCarpeta = Path.Combine(
                            Directory.GetCurrentDirectory(),
                            "wwwroot",
                            "images",
                            "perfumes");

                        if (!Directory.Exists(rutaCarpeta))
                        {
                            Directory.CreateDirectory(rutaCarpeta);
                        }

                        string rutaCompleta = Path.Combine(rutaCarpeta, nombreArchivo);

                        using (var stream = new FileStream(rutaCompleta, FileMode.Create))
                        {
                            await archivoImagen.CopyToAsync(stream);
                        }

                        productoExistente.ImagenUrl = "/images/perfumes/" + nombreArchivo;
                    }

                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Productos.Any(e => e.IdProducto == id))
                    {
                        return NotFound();
                    }

                    throw;
                }
            }

            ViewData["IdMarca"] = new SelectList(
                _context.Marcas, "IdMarca", "Nombre", producto.IdMarca);

            ViewData["IdCategoria"] = new SelectList(
                _context.Categorias, "IdCategoria", "Nombre", producto.IdCategoria);

            // Conservamos la imagen actual si hay errores de validación.
            producto.ImagenUrl = productoExistente.ImagenUrl;

            return View(producto);
        }

        //GET : Productos/Delete/5
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Delete(int id) 
        {
            if (id == null) 
            { 
                return NotFound();
            }

            var producto = await _context.Productos
                .Include(p => p.Marca)
                .Include(p => p.Categoria)
                .FirstOrDefaultAsync(p => p.IdProducto == id);

            if (producto == null) 
            {
                return NotFound();
            }
            return View(producto);
        }

        //POST : Productos/Delete/5

        [Authorize(Roles = "Administrador")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var producto = await _context.Productos.FindAsync(id);

            if (producto == null)
            {
                return NotFound();
            }

            // Eliminar la imagen solamente si pertenece a la carpeta de perfumes.
            if (!string.IsNullOrWhiteSpace(producto.ImagenUrl) &&
                producto.ImagenUrl.StartsWith(
                    "/images/perfumes/",
                    StringComparison.OrdinalIgnoreCase))
            {
                string rutaRelativa = producto.ImagenUrl.TrimStart('/')
                    .Replace('/', Path.DirectorySeparatorChar);

                string rutaFisica = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot",
                    rutaRelativa);

                if (System.IO.File.Exists(rutaFisica))
                {
                    System.IO.File.Delete(rutaFisica);
                }
            }

            _context.Productos.Remove(producto);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


    }
}
