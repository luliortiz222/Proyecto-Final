using Microsoft.AspNetCore.Mvc;
using ProyectoIntegradorFinal.Models;
using Microsoft.EntityFrameworkCore;
using ProyectoIntegradorFinal.Data;

namespace ProyectoIntegradorFinal.Controllers
{
    public class PagosController : Controller
    {
        private readonly AppDbContext _context;

        public PagosController(AppDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            var pagos = await _context.Pagos.Include(p => p.Orden).ThenInclude(o => o.Usuario).OrderByDescending(p => p.FechaPago).ToListAsync();
            return View(pagos);
        }

        // GET: Pagos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var pago = await _context.Pagos
                .Include(p => p.Orden)
                .FirstOrDefaultAsync(m => m.IdPago == id);

            if (pago == null) return NotFound();

            return View(pago);
        }
    }
}
