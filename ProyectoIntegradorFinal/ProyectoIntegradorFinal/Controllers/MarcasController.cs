using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoIntegradorFinal.Models;
using ProyectoIntegradorFinal.Data;
namespace ProyectoIntegradorFinal.Controllers
{
    public class MarcasController : Controller
    {
        private readonly AppDbContext _context;
        public MarcasController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var marcas = await _context.Marcas.ToListAsync();
            return View(marcas);
        }

        //GET : Marcas/Create
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Nombre")] Marca marca)
        {
            if (ModelState.IsValid)
            {
                _context.Add(marca);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(marca);
        }
    }
}
