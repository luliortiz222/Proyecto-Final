using Microsoft.AspNetCore.Mvc;
using ProyectoIntegradorFinal.Data;
using ProyectoIntegradorFinal.Models;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace ProyectoIntegradorFinal.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? pagina)
        {
            int pageSize = 4;
            int pageNumber = pagina ?? 1;

            var query = _context.Productos
                .Include(p => p.Marca)
                .Include(p => p.Categoria)
                .AsQueryable();

            // Contar todos los productos del catálogo
            int totalRegistros = await query.CountAsync();

            // Calcular el total de páginas
            int totalPaginas = (int)Math.Ceiling(
                (double)totalRegistros / pageSize
            );

            // Evitar números de página inválidos
            if (pageNumber < 1)
                pageNumber = 1;

            if (totalPaginas > 0 && pageNumber > totalPaginas)
                pageNumber = totalPaginas;

            // Obtener solamente los 4 productos de la página actual
            var perfumes = await query
                .OrderBy(p => p.Nombre)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var categorias = await _context.Categorias
                .OrderBy(c => c.Nombre)
                .ToListAsync();

            ViewBag.Categorias = categorias;
            ViewBag.PaginaActual = pageNumber;
            ViewBag.TotalPaginas = totalPaginas;
            ViewBag.TotalRegistros = totalRegistros;

            return View(perfumes);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(
            Duration = 0,
            Location = ResponseCacheLocation.None,
            NoStore = true
        )]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id
                    ?? HttpContext.TraceIdentifier
            });
        }
    }
}