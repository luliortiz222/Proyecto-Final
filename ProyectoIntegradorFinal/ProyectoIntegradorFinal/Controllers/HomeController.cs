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

            int pageSize = 8;
            int pageNumber = pagina ?? 1;

            var query = _context.Productos
                .Include(p => p.Marca)
                .Include(p => p.Categoria)
                .AsQueryable();

            int totalRegistros = await query.CountAsync();

            var perfumes = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();



            var categorias = await _context.Categorias
                .OrderBy(c => c.Nombre)
                .ToListAsync();

            ViewBag.Categorias = categorias;
            ViewBag.PaginaActual = pageNumber;
            ViewBag.TotalPaginas = (int)Math.Ceiling((double)totalRegistros / pageSize);

            return View(perfumes);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
