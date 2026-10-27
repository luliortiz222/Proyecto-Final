using System;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoIntegradorFinal.Data;
using ProyectoIntegradorFinal.Models;

namespace ProyectoIntegradorFinal.Controllers
{
    public class PagosController : Controller
    {
        private readonly AppDbContext _context;
        private const int RegistrosPorPagina = 2;

        public PagosController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Pagos
        // Solo el administrador puede consultar todos los pagos.
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Index(
            string? buscar,
            string? metodoPago,
            int page = 1)
        {
            buscar = buscar?.Trim();
            metodoPago = metodoPago?.Trim();

            var consulta = _context.Pagos
                .AsNoTracking()
                .Include(p => p.Orden)
                    .ThenInclude(o => o.Usuario)
                .AsQueryable();

            // Búsqueda por número de pago, orden o cliente.
            if (!string.IsNullOrWhiteSpace(buscar))
            {
                bool esNumero = int.TryParse(buscar, out int numero);

                consulta = consulta.Where(p =>
                    (esNumero &&
                        (p.IdPago == numero || p.IdOrden == numero))
                    || (p.Orden != null &&
                        p.Orden.Usuario != null &&
                        (
                            p.Orden.Usuario.Nombre.Contains(buscar) ||
                            p.Orden.Usuario.Apellido.Contains(buscar)
                        ))
                );
            }

            // Filtro por método de pago.
            if (!string.IsNullOrWhiteSpace(metodoPago))
            {
                consulta = consulta.Where(
                    p => p.MetodoPago == metodoPago
                );
            }

            // Métodos disponibles para el selector.
            ViewBag.MetodosPago = await _context.Pagos
                .AsNoTracking()
                .Where(p => p.MetodoPago != null &&
                            p.MetodoPago != "")
                .Select(p => p.MetodoPago!)
                .Distinct()
                .OrderBy(m => m)
                .ToListAsync();

            int totalRegistros = await consulta.CountAsync();

            int totalPaginas = Math.Max(
                1,
                (int)Math.Ceiling(
                    totalRegistros / (double)RegistrosPorPagina
                )
            );

            page = Math.Clamp(page, 1, totalPaginas);

            var pagos = await consulta
                .OrderByDescending(p => p.FechaPago)
                .ThenByDescending(p => p.IdPago)
                .Skip((page - 1) * RegistrosPorPagina)
                .Take(RegistrosPorPagina)
                .ToListAsync();

            ViewBag.Buscar = buscar ?? "";
            ViewBag.MetodoPago = metodoPago ?? "";
            ViewBag.PaginaActual = page;
            ViewBag.TotalPaginas = totalPaginas;
            ViewBag.TotalRegistros = totalRegistros;

            return View(pagos);
        }

        // GET: Pagos/MisPagos
        // Cada cliente ve solamente los pagos de sus propias órdenes.

        [Authorize(Roles = "Cliente")]
        public async Task<IActionResult> MisPagos(
            string? metodoPago,
            string? estadoPago,
            int page = 1)
        {
            const int pageSize = 2;

            var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(idClaim, out int idUsuario))
                return Challenge();

            metodoPago = metodoPago?.Trim();
            estadoPago = estadoPago?.Trim();

            // La consulta siempre se limita al cliente autenticado.
            var consulta = _context.Pagos
                .AsNoTracking()
                .Where(p => p.Orden.IdUsuario == idUsuario);

            // Buscar método de pago desde el servidor.
            if (!string.IsNullOrWhiteSpace(metodoPago))
            {
                consulta = consulta.Where(p =>
                    p.MetodoPago != null &&
                    p.MetodoPago.Contains(metodoPago));
            }

            // Filtrar por estado.
            if (!string.IsNullOrWhiteSpace(estadoPago))
            {
                consulta = consulta.Where(p =>
                    p.EstadoPago == estadoPago);
            }

            int totalRegistros = await consulta.CountAsync();

            int totalPaginas = Math.Max(
                1,
                (int)Math.Ceiling(totalRegistros / (double)pageSize)
            );

            page = Math.Clamp(page, 1, totalPaginas);

            var pagos = await consulta
                .OrderByDescending(p => p.FechaPago)
                .ThenByDescending(p => p.IdPago)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.PaginaActual = page;
            ViewBag.TotalPaginas = totalPaginas;
            ViewBag.TotalRegistros = totalRegistros;
            ViewBag.MetodoPago = metodoPago ?? "";
            ViewBag.EstadoPago = estadoPago ?? "";

            return View(pagos);
        }


        // GET: Pagos/Details/5
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
        public async Task<IActionResult> CambiarEstado(
            int id,
            string estadoPago)
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
                TempData["Error"] =
                    "El estado de pago seleccionado no es válido.";

                return RedirectToAction(nameof(Details), new { id });
            }

            pago.EstadoPago = estadoPago;

            await _context.SaveChangesAsync();

            TempData["Mensaje"] =
                "El estado del pago se actualizó correctamente.";

            return RedirectToAction(nameof(Details), new { id });
        }
    }
}
