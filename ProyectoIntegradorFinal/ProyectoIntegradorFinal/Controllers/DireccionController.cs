using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using ProyectoIntegradorFinal.Data;
using ProyectoIntegradorFinal.Models;
using System.Security.Claims;

namespace ProyectoIntegradorFinal.Controllers
{
    [Authorize(Roles = "Cliente")]
    public class DireccionController : Controller
    {
        private readonly AppDbContext _context;

        public DireccionController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Direccion/Index
        public async Task<IActionResult> Index()
        {
            int idUsuario = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!
            );

            var direcciones = await _context.Direccion
                .Where(d => d.IdUsuario == idUsuario)
                .ToListAsync();

            return View(direcciones);
        }

        // GET: Direccion/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Direccion/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Direccion direccion)
        {
            int idUsuario = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!
            );

            // Asociamos la dirección al usuario autenticado.
            direccion.IdUsuario = idUsuario;

            // El IdUsuario se obtiene de la sesión, no del formulario.
            ModelState.Remove(nameof(Direccion.IdUsuario));
            ModelState.Remove(nameof(Direccion.Usuario));

            if (!ModelState.IsValid)
                return View(direccion);

            _context.Direccion.Add(direccion);
            await _context.SaveChangesAsync();

            TempData["Mensaje"] = "Dirección agregada correctamente.";

            return RedirectToAction(nameof(Index));
        }

        // GET: Direccion/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            int idUsuario = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!
            );

            var direccion = await _context.Direccion
                .FirstOrDefaultAsync(d =>
                    d.IdDireccion == id &&
                    d.IdUsuario == idUsuario);

            if (direccion == null)
                return NotFound();

            return View(direccion);
        }

        // POST: Direccion/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            Direccion direccion)
        {
            if (id != direccion.IdDireccion)
                return NotFound();

            int idUsuario = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!
            );

            var direccionExistente = await _context.Direccion
                .FirstOrDefaultAsync(d =>
                    d.IdDireccion == id &&
                    d.IdUsuario == idUsuario);

            if (direccionExistente == null)
                return NotFound();

            ModelState.Remove(nameof(Direccion.IdUsuario));
            ModelState.Remove(nameof(Direccion.Usuario));

            if (!ModelState.IsValid)
                return View(direccion);

            // Actualizamos solo los campos permitidos.
            direccionExistente.Calle = direccion.Calle;
            direccionExistente.Numero = direccion.Numero;
            direccionExistente.Ciudad = direccion.Ciudad;
            direccionExistente.Provincia = direccion.Provincia;
            direccionExistente.CodigoPostal = direccion.CodigoPostal;

            await _context.SaveChangesAsync();

            TempData["Mensaje"] = "Dirección modificada correctamente.";

            return RedirectToAction(nameof(Index));
        }

        // GET: Direccion/Delete/5
        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            int idUsuario = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!
            );

            var direccion = await _context.Direccion
                .FirstOrDefaultAsync(d =>
                    d.IdDireccion == id &&
                    d.IdUsuario == idUsuario);

            if (direccion == null)
                return NotFound();

            return View(direccion);
        }

        // POST: Direccion/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            int idUsuario = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!
            );

            var direccion = await _context.Direccion
                .FirstOrDefaultAsync(d =>
                    d.IdDireccion == id &&
                    d.IdUsuario == idUsuario);

            if (direccion == null)
                return NotFound();

            _context.Direccion.Remove(direccion);
            await _context.SaveChangesAsync();

            TempData["Mensaje"] = "Dirección eliminada correctamente.";

            return RedirectToAction(nameof(Index));
        }
    }
}