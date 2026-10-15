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
        public async Task<IActionResult> Index()
        {
            var perfumes = await _context.Productos.Include(p => p.Marca).Include(p => p.Categoria).ToListAsync();
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
