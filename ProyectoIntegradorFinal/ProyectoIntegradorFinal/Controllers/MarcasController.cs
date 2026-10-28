
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoIntegradorFinal.Data;
using ProyectoIntegradorFinal.Models;

namespace ProyectoIntegradorFinal.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class MarcasController : Controller
    {
        private readonly AppDbContext _context;

        private const int RegistrosPorPagina = 5;

        public MarcasController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? buscar, int pagina = 1)
        {
            buscar = buscar?.Trim();

            var consulta = _context.Marcas.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                consulta = consulta.Where(m => m.Nombre.Contains(buscar));
            }

            int totalRegistros = await consulta.CountAsync();

            int totalPaginas = (int)Math.Ceiling(
                totalRegistros / (double)RegistrosPorPagina
            );

            pagina = totalPaginas > 0
                ? Math.Clamp(pagina, 1, totalPaginas)
                : 1;

            var marcas = await consulta
                .OrderBy(m => m.Nombre)
                .Skip((pagina - 1) * RegistrosPorPagina)
                .Take(RegistrosPorPagina)
                .ToListAsync();

            ViewBag.Buscar = buscar;
            ViewBag.PaginaActual = pagina;
            ViewBag.TotalPaginas = totalPaginas;
            ViewBag.TotalRegistros = totalRegistros;

            return View(marcas);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("Nombre")] Marca marca)
        {
            if (!ModelState.IsValid)
                return View(marca);

            _context.Marcas.Add(marca);
            await _context.SaveChangesAsync();

            TempData["Mensaje"] = "La marca se creó correctamente.";

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var marca = await _context.Marcas
                .FirstOrDefaultAsync(m => m.IdMarca == id);

            if (marca == null)
                return NotFound();

            return View(marca);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("IdMarca,Nombre")] Marca marca)
        {
            if (id != marca.IdMarca)
                return NotFound();

            if (!ModelState.IsValid)
                return View(marca);

            try
            {
                _context.Update(marca);
                await _context.SaveChangesAsync();

                TempData["Mensaje"] = "La marca se actualizó correctamente.";

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                bool existe = await _context.Marcas
                    .AnyAsync(m => m.IdMarca == marca.IdMarca);

                if (!existe)
                    return NotFound();

                throw;
            }
        }

        [HttpGet]
        public async Task<IActionResult> Sugerencias(string? termino)
        {
            termino = termino?.Trim();

            if (string.IsNullOrWhiteSpace(termino))
                return Json(new List<string>());

            var marcas = await _context.Marcas
                .AsNoTracking()
                .Where(m => m.Nombre.Contains(termino))
                .OrderBy(m => m.Nombre)
                .Select(m => m.Nombre)
                .Take(8)
                .ToListAsync();

            return Json(marcas);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var marca = await _context.Marcas
                .Include(m => m.Productos)
                .FirstOrDefaultAsync(m => m.IdMarca == id);

            if (marca == null)
                return NotFound();

            ViewBag.CantidadProductos = marca.Productos?.Count ?? 0;

            return View(marca);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var marca = await _context.Marcas
                .Include(m => m.Productos)
                .FirstOrDefaultAsync(m => m.IdMarca == id);

            if (marca == null)
                return NotFound();

            if (marca.Productos != null && marca.Productos.Any())
            {
                TempData["Mensaje"] =
                    "No se puede eliminar la marca porque tiene perfumes asociados.";

                return RedirectToAction(nameof(Index));
            }

            _context.Marcas.Remove(marca);
            await _context.SaveChangesAsync();

            TempData["Mensaje"] = "La marca se eliminó correctamente.";

            return RedirectToAction(nameof(Index));
        }
    }
}
