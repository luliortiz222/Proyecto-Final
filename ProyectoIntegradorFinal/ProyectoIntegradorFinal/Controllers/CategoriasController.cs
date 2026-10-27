
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoIntegradorFinal.Data;
using ProyectoIntegradorFinal.Models;

namespace ProyectoIntegradorFinal.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class CategoriasController : Controller
    {
        private readonly AppDbContext _context;

        private const int RegistrosPorPagina = 4;

        public CategoriasController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? buscar, int pagina = 1)
        {
            buscar = buscar?.Trim();

            var consulta = _context.Categorias.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                consulta = consulta.Where(c => c.Nombre.Contains(buscar));
            }

            int totalRegistros = await consulta.CountAsync();

            int totalPaginas = (int)Math.Ceiling(
                totalRegistros / (double)RegistrosPorPagina
            );

            if (totalPaginas > 0)
            {
                pagina = Math.Clamp(pagina, 1, totalPaginas);
            }
            else
            {
                pagina = 1;
            }

            var categorias = await consulta
                .OrderBy(c => c.Nombre)
                .Skip((pagina - 1) * RegistrosPorPagina)
                .Take(RegistrosPorPagina)
                .ToListAsync();

            ViewBag.Buscar = buscar;
            ViewBag.PaginaActual = pagina;
            ViewBag.TotalPaginas = totalPaginas;
            ViewBag.TotalRegistros = totalRegistros;

            return View(categorias);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("Nombre")] Categoria categoria)
        {
            if (!ModelState.IsValid)
                return View(categoria);

            _context.Categorias.Add(categoria);
            await _context.SaveChangesAsync();

            TempData["Mensaje"] = "La categoría se creó correctamente.";

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var categoria = await _context.Categorias
                .FirstOrDefaultAsync(c => c.IdCategoria == id);

            if (categoria == null)
                return NotFound();

            return View(categoria);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("IdCategoria,Nombre")] Categoria categoria)
        {
            if (id != categoria.IdCategoria)
                return NotFound();

            if (!ModelState.IsValid)
                return View(categoria);

            try
            {
                _context.Update(categoria);
                await _context.SaveChangesAsync();

                TempData["Mensaje"] = "La categoría se actualizó correctamente.";

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                bool existe = await _context.Categorias
                    .AnyAsync(c => c.IdCategoria == categoria.IdCategoria);

                if (!existe)
                    return NotFound();

                throw;
            }
        }
    }
}
