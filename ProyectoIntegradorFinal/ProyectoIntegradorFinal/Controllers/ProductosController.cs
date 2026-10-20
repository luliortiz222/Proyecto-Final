using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;    
using ProyectoIntegradorFinal.Data;
using ProyectoIntegradorFinal.Models;

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
        public async Task<IActionResult> Index()
        {
            
            var perfumes = await _context.Productos.Include(p=>p.Marca).Include(p=>p.Categoria).ToListAsync();

            return View(perfumes);
        }

        //GET : Productos/Create
        public IActionResult Create() 
        {
            ViewData["IdMarca"] = new SelectList(_context.Marcas, "IdMarca", "Nombre");
            ViewData["IdCategoria"] = new SelectList(_context.Categorias, "IdCategoria", "Nombre");
            return View();
        }


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


    }
}
