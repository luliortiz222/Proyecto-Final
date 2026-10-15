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
        public async Task<IActionResult> Create([Bind("Nombre,Descripcion,Precio,Stock,VolumenMl,Concentracion,ImagenUrl,IdMarca,IdCategoria")] Producto producto) 
        {
            if (ModelState.IsValid)
            {
                _context.Add(producto);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["IdMarca"] = new SelectList(_context.Marcas, "IdMarca", "Nombre", producto.IdMarca);
            ViewData["IdCategoria"] = new SelectList(_context.Categorias, "IdCategoria", "Nombre", producto.IdCategoria);

            return View(producto);
        }


    }
}
