
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        // GET: Productos
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Index(
            string? buscar,
            int? idCategoria,
            int? idMarca,
            int? pagina)
        {
            int pageSize = 4;
            int pageNumber = pagina ?? 1;

            if (pageNumber < 1)
                pageNumber = 1;

            var consulta = _context.Productos
                .Include(p => p.Marca)
                .Include(p => p.Categoria)
                .AsQueryable();

            // Buscar productos por nombre o marca.
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

            // Filtrar por categoría.
            if (idCategoria.HasValue)
            {
                consulta = consulta.Where(p =>
                    p.IdCategoria == idCategoria.Value);
            }

            // Filtrar por marca.
            if (idMarca.HasValue)
            {
                consulta = consulta.Where(p =>
                    p.IdMarca == idMarca.Value);
            }

            int totalRegistros = await consulta.CountAsync();

            var productos = await consulta
                .OrderBy(p => p.Nombre)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Buscar = buscar;
            ViewBag.IdCategoria = idCategoria;
            ViewBag.IdMarca = idMarca;
            ViewBag.PaginaActual = pageNumber;
            ViewBag.TotalPaginas =
                (int)Math.Ceiling((double)totalRegistros / pageSize);

            // Se conservan para los filtros del catálogo en Index.
            ViewBag.Categorias = await _context.Categorias
                .OrderBy(c => c.Nombre)
                .ToListAsync();

            ViewBag.Marcas = await _context.Marcas
                .OrderBy(m => m.Nombre)
                .ToListAsync();

            return View(productos);
        }

        // GET: Productos/BuscarMarcas?termino=...
        [Authorize(Roles = "Administrador")]
        [HttpGet]
        public async Task<IActionResult> BuscarMarcas(string? termino)
        {
            if (string.IsNullOrWhiteSpace(termino))
                return Json(Array.Empty<object>());

            termino = termino.Trim();

            var marcas = await _context.Marcas
                .AsNoTracking()
                .Where(m => m.Nombre.Contains(termino))
                .OrderBy(m => m.Nombre)
                .Take(10)
                .Select(m => new
                {
                    id = m.IdMarca,
                    nombre = m.Nombre
                })
                .ToListAsync();

            return Json(marcas);
        }

        // GET: Productos/BuscarCategorias?termino=...
        [Authorize(Roles = "Administrador")]
        [HttpGet]
        public async Task<IActionResult> BuscarCategorias(string? termino)
        {
            if (string.IsNullOrWhiteSpace(termino))
                return Json(Array.Empty<object>());

            termino = termino.Trim();

            var categorias = await _context.Categorias
                .AsNoTracking()
                .Where(c => c.Nombre.Contains(termino))
                .OrderBy(c => c.Nombre)
                .Take(10)
                .Select(c => new
                {
                    id = c.IdCategoria,
                    nombre = c.Nombre
                })
                .ToListAsync();

            return Json(categorias);
        }

        // GET: Productos/Create
        [Authorize(Roles = "Administrador")]
        [HttpGet]
        public IActionResult Create()
        {
            // Ya no se cargan todos los registros en SelectList.
            return View();
        }

        // POST: Productos/Create
        [Authorize(Roles = "Administrador")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("Nombre,Descripcion,Precio,Stock,VolumenMl,Concentracion,IdMarca,IdCategoria")]
            Producto producto,
            IFormFile? archivoImagen)
        {
            // Comprobar que los IDs elegidos existan.
            if (producto.IdMarca <= 0 ||
                !await _context.Marcas.AnyAsync(m => m.IdMarca == producto.IdMarca))
            {
                ModelState.AddModelError(
                    nameof(producto.IdMarca),
                    "Seleccioná una marca válida.");
            }

            if (producto.IdCategoria <= 0 ||
                !await _context.Categorias.AnyAsync(c => c.IdCategoria == producto.IdCategoria))
            {
                ModelState.AddModelError(
                    nameof(producto.IdCategoria),
                    "Seleccioná una categoría válida.");
            }

            if (ModelState.IsValid)
            {
                if (archivoImagen != null && archivoImagen.Length > 0)
                {
                    string extension =
                        Path.GetExtension(archivoImagen.FileName).ToLowerInvariant();

                    string[] extensionesPermitidas =
                        { ".jpg", ".jpeg", ".png", ".webp" };

                    if (!extensionesPermitidas.Contains(extension))
                    {
                        ModelState.AddModelError(
                            "archivoImagen",
                            "La imagen debe ser JPG, JPEG, PNG o WEBP.");
                    }
                    else if (archivoImagen.Length > 2 * 1024 * 1024)
                    {
                        ModelState.AddModelError(
                            "archivoImagen",
                            "La imagen no puede superar los 2 MB.");
                    }
                    else
                    {
                        string nombreArchivo = Guid.NewGuid() + extension;

                        string rutaCarpeta = Path.Combine(
                            Directory.GetCurrentDirectory(),
                            "wwwroot",
                            "images",
                            "perfumes");

                        Directory.CreateDirectory(rutaCarpeta);

                        string rutaCompleta =
                            Path.Combine(rutaCarpeta, nombreArchivo);

                        using (var stream = new FileStream(
                            rutaCompleta, FileMode.Create))
                        {
                            await archivoImagen.CopyToAsync(stream);
                        }

                        producto.ImagenUrl =
                            "/images/perfumes/" + nombreArchivo;
                    }
                }

                if (ModelState.IsValid)
                {
                    _context.Productos.Add(producto);
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }
            }

            // Mantener los nombres seleccionados si hay errores.
            ViewBag.MarcaSeleccionada = await _context.Marcas
                .Where(m => m.IdMarca == producto.IdMarca)
                .Select(m => m.Nombre)
                .FirstOrDefaultAsync() ?? "";

            ViewBag.CategoriaSeleccionada = await _context.Categorias
                .Where(c => c.IdCategoria == producto.IdCategoria)
                .Select(c => c.Nombre)
                .FirstOrDefaultAsync() ?? "";

            return View(producto);
        }

        // GET: Productos/Edit/5
        [Authorize(Roles = "Administrador")]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (id <= 0)
                return NotFound();

            var producto = await _context.Productos
                .Include(p => p.Marca)
                .Include(p => p.Categoria)
                .FirstOrDefaultAsync(p => p.IdProducto == id);

            if (producto == null)
                return NotFound();

            ViewBag.MarcaSeleccionada = producto.Marca?.Nombre ?? "";
            ViewBag.CategoriaSeleccionada = producto.Categoria?.Nombre ?? "";

            return View(producto);
        }

        // POST: Productos/Edit/5
        [Authorize(Roles = "Administrador")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("IdProducto,Nombre,Descripcion,Precio,Stock,VolumenMl,Concentracion,IdMarca,IdCategoria")]
            Producto producto,
            IFormFile? archivoImagen)
        {
            if (id != producto.IdProducto)
                return NotFound();

            var productoExistente =
                await _context.Productos.FindAsync(id);

            if (productoExistente == null)
                return NotFound();

            // Comprobar que la marca y la categoría existan.
            if (producto.IdMarca <= 0 ||
                !await _context.Marcas.AnyAsync(m => m.IdMarca == producto.IdMarca))
            {
                ModelState.AddModelError(
                    nameof(producto.IdMarca),
                    "Seleccioná una marca válida.");
            }

            if (producto.IdCategoria <= 0 ||
                !await _context.Categorias.AnyAsync(c => c.IdCategoria == producto.IdCategoria))
            {
                ModelState.AddModelError(
                    nameof(producto.IdCategoria),
                    "Seleccioná una categoría válida.");
            }

            if (ModelState.IsValid)
            {
                string? nuevaImagenUrl = null;

                if (archivoImagen != null && archivoImagen.Length > 0)
                {
                    string extension =
                        Path.GetExtension(archivoImagen.FileName).ToLowerInvariant();

                    string[] extensionesPermitidas =
                        { ".jpg", ".jpeg", ".png", ".webp" };

                    if (!extensionesPermitidas.Contains(extension))
                    {
                        ModelState.AddModelError(
                            "archivoImagen",
                            "La imagen debe ser JPG, JPEG, PNG o WEBP.");
                    }
                    else if (archivoImagen.Length > 2 * 1024 * 1024)
                    {
                        ModelState.AddModelError(
                            "archivoImagen",
                            "La imagen no puede superar los 2 MB.");
                    }
                    else
                    {
                        string nombreArchivo = Guid.NewGuid() + extension;

                        string rutaCarpeta = Path.Combine(
                            Directory.GetCurrentDirectory(),
                            "wwwroot",
                            "images",
                            "perfumes");

                        Directory.CreateDirectory(rutaCarpeta);

                        string rutaCompleta =
                            Path.Combine(rutaCarpeta, nombreArchivo);

                        using (var stream = new FileStream(
                            rutaCompleta, FileMode.Create))
                        {
                            await archivoImagen.CopyToAsync(stream);
                        }

                        nuevaImagenUrl =
                            "/images/perfumes/" + nombreArchivo;
                    }
                }

                if (ModelState.IsValid)
                {
                    productoExistente.Nombre = producto.Nombre;
                    productoExistente.Descripcion = producto.Descripcion;
                    productoExistente.Precio = producto.Precio;
                    productoExistente.Stock = producto.Stock;
                    productoExistente.VolumenMl = producto.VolumenMl;
                    productoExistente.Concentracion = producto.Concentracion;
                    productoExistente.IdMarca = producto.IdMarca;
                    productoExistente.IdCategoria = producto.IdCategoria;

                    // Si no se subió una nueva imagen, se conserva la actual.
                    if (nuevaImagenUrl != null)
                    {
                        productoExistente.ImagenUrl = nuevaImagenUrl;
                    }

                    try
                    {
                        await _context.SaveChangesAsync();
                        return RedirectToAction(nameof(Index));
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!await _context.Productos
                            .AnyAsync(p => p.IdProducto == id))
                        {
                            return NotFound();
                        }

                        throw;
                    }
                }
            }

            // Recuperar nombres para volver a mostrarlos si falla la validación.
            ViewBag.MarcaSeleccionada = await _context.Marcas
                .Where(m => m.IdMarca == producto.IdMarca)
                .Select(m => m.Nombre)
                .FirstOrDefaultAsync() ?? "";

            ViewBag.CategoriaSeleccionada = await _context.Categorias
                .Where(c => c.IdCategoria == producto.IdCategoria)
                .Select(c => c.Nombre)
                .FirstOrDefaultAsync() ?? "";

            // Conservar la imagen existente.
            producto.ImagenUrl = productoExistente.ImagenUrl;

            return View(producto);
        }

        // GET: Productos/Delete/5
        [Authorize(Roles = "Administrador")]
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
                return NotFound();

            var producto = await _context.Productos
                .Include(p => p.Marca)
                .Include(p => p.Categoria)
                .FirstOrDefaultAsync(p => p.IdProducto == id);

            if (producto == null)
                return NotFound();

            // No permitir iniciar la eliminación si el producto
            // ya está asociado a una orden.
            bool tieneOrdenes = await _context.DetallesOrden
                .AnyAsync(d => d.IdProducto == id);

            if (tieneOrdenes)
            {
                TempData["Error"] =
                    "No se puede eliminar el perfume porque está asociado a una o más órdenes.";

                return RedirectToAction(nameof(Index));
            }

            return View(producto);
        }

        // POST: Productos/Delete/5
        [Authorize(Roles = "Administrador")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var producto = await _context.Productos
                .FirstOrDefaultAsync(p => p.IdProducto == id);

            if (producto == null)
                return NotFound();

            // Volver a comprobar antes de eliminar.
            // Esta validación también protege los envíos directos del formulario.
            bool tieneOrdenes = await _context.DetallesOrden
                .AnyAsync(d => d.IdProducto == id);

            if (tieneOrdenes)
            {
                TempData["Error"] =
                    "No se puede eliminar el perfume porque está asociado a una o más órdenes.";

                return RedirectToAction(nameof(Index));
            }

            string? rutaFisicaImagen = null;

            // Preparar la ruta de la imagen, sin borrarla todavía.
            if (!string.IsNullOrWhiteSpace(producto.ImagenUrl) &&
                producto.ImagenUrl.StartsWith(
                    "/images/perfumes/",
                    StringComparison.OrdinalIgnoreCase))
            {
                string rutaRelativa = producto.ImagenUrl.TrimStart('/')
                    .Replace('/', Path.DirectorySeparatorChar);

                rutaFisicaImagen = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot",
                    rutaRelativa);
            }

            _context.Productos.Remove(producto);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                TempData["Error"] =
                    "No se pudo eliminar el perfume porque tiene registros asociados.";

                return RedirectToAction(nameof(Index));
            }

            // Borrar la imagen únicamente después de confirmar
            // que la eliminación del producto se guardó correctamente.
            if (!string.IsNullOrWhiteSpace(rutaFisicaImagen) &&
                System.IO.File.Exists(rutaFisicaImagen))
            {
                try
                {
                    System.IO.File.Delete(rutaFisicaImagen);
                }
                catch (IOException)
                {
                    // El producto ya fue eliminado de la base de datos.
                    // Un fallo al borrar el archivo no debe revertir la operación.
                }
                catch (UnauthorizedAccessException)
                {
                    // No impedir la operación por un problema de permisos del archivo.
                }
            }

            TempData["Mensaje"] = "Perfume eliminado correctamente.";

            return RedirectToAction(nameof(Index));
        }

    }
}
