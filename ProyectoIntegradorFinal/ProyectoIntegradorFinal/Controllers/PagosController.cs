using Microsoft.AspNetCore.Mvc;
using ProyectoIntegradorFinal.Models;
using Microsoft.EntityFrameworkCore;

using ProyectoIntegradorFinal.Data;
using ProyectoIntegradorFinal.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ProyectoIntegradorFinal.Controllers
{
    public class PagosController : Controller
    {
        private readonly AppDbContext _context;

        public PagosController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Pagos
        // Solo el administrador puede ver todos los pagos.
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Index(int page = 1)
        {
            const int pageSize = 10;

            if (page < 1)
                page = 1;

            var consulta = _context.Pagos
                .Include(p => p.Orden)
                    .ThenInclude(o => o.Usuario)
                .OrderByDescending(p => p.FechaPago)
                .ThenByDescending(p => p.IdPago);

            int totalRegistros = await consulta.CountAsync();

            int totalPaginas = Math.Max(
                1,
                (int)Math.Ceiling(totalRegistros / (double)pageSize)
            );

            if (page > totalPaginas)
                page = totalPaginas;

            var pagos = await consulta
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.PaginaActual = page;
            ViewBag.TotalPaginas = totalPaginas;
            ViewBag.TotalRegistros = totalRegistros;

            return View(pagos);
        }

        // GET: Pagos/MisPagos
        // Cada cliente ve solamente los pagos de sus propias órdenes.
        [Authorize(Roles = "Cliente")]
        public async Task<IActionResult> MisPagos(int page = 1)
        {
            const int pageSize = 10;

            var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(idClaim, out int idUsuario))
                return Challenge();

            if (page < 1)
                page = 1;

            var consulta = _context.Pagos
                .Where(p => p.Orden.IdUsuario == idUsuario)
                .Include(p => p.Orden)
                .OrderByDescending(p => p.FechaPago)
                .ThenByDescending(p => p.IdPago);

            int totalRegistros = await consulta.CountAsync();

            int totalPaginas = Math.Max(
                1,
                (int)Math.Ceiling(totalRegistros / (double)pageSize)
            );

            if (page > totalPaginas)
                page = totalPaginas;

            var pagos = await consulta
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.PaginaActual = page;
            ViewBag.TotalPaginas = totalPaginas;
            ViewBag.TotalRegistros = totalRegistros;

            return View(pagos);
        }

        // GET: Pagos/Details/5
        // Solo el administrador puede consultar el detalle.
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var pago = await _context.Pagos
                .Include(p => p.Orden)
                    .ThenInclude(o => o.Usuario)
                .FirstOrDefaultAsync(p => p.IdPago == id);

            if (pago == null)
                return NotFound();

            return View(pago);
        }

        // POST: Pagos/CambiarEstado

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> CambiarEstado(int id, string estadoPago)
        {
            var pago = await _context.Pagos
                .FirstOrDefaultAsync(p => p.IdPago == id);

            if (pago == null)
                return NotFound();

            var estadosPermitidos = new[]
            {
        "Pendiente",
        "En revisión",
        "Aprobado",
        "Rechazado"
    };

            if (string.IsNullOrWhiteSpace(estadoPago) ||
                !estadosPermitidos.Contains(estadoPago))
            {
                TempData["Error"] = "El estado de pago seleccionado no es válido.";
                return RedirectToAction(nameof(Details), new { id });
            }

            // El administrador solo puede modificar el estado.
            // MetodoPago conserva el valor elegido por el cliente.
            pago.EstadoPago = estadoPago;

            await _context.SaveChangesAsync();

            TempData["Mensaje"] = "El estado del pago se actualizó correctamente.";

            return RedirectToAction(nameof(Details), new { id });
        }


    }
}
