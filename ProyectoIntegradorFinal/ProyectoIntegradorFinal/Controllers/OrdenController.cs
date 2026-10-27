using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using ProyectoIntegradorFinal.Data;
using ProyectoIntegradorFinal.Models;
using System.Security.Claims;

namespace ProyectoIntegradorFinal.Controllers
{
    [Authorize(Roles = "Cliente,Administrador")]
    public class OrdenController : Controller
    {
        private readonly AppDbContext _context;
        private const int RegistrosPorPagina = 3;

        public OrdenController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Orden/Index

        public async Task<IActionResult> Index(
            int pagina = 1,
            string? direccion = null,
            string? estado = null)
        {
            if (pagina < 1)
                pagina = 1;

            // Consulta inicial en el servidor.
            var consulta = _context.Ordenes
                .Include(o => o.Usuario)
                .Include(o => o.Direccion)
                .AsQueryable();

            // Seguridad: cada cliente consulta únicamente sus órdenes.
            if (User.IsInRole("Cliente"))
            {
                var valorId = User.FindFirstValue(
                    ClaimTypes.NameIdentifier
                );

                if (!int.TryParse(valorId, out int idUsuario))
                    return Forbid();

                consulta = consulta.Where(
                    o => o.IdUsuario == idUsuario
                );
            }

            // Búsqueda por calle o ciudad.
            if (!string.IsNullOrWhiteSpace(direccion))
            {
                string termino = direccion.Trim();

                consulta = consulta.Where(o =>
                    o.Direccion != null &&
                    (
                        (o.Direccion.Calle != null &&
                         o.Direccion.Calle.Contains(termino))
                        ||
                        (o.Direccion.Ciudad != null &&
                         o.Direccion.Ciudad.Contains(termino))
                    )
                );
            }

            // Búsqueda por estado seleccionado.
            if (!string.IsNullOrWhiteSpace(estado))
            {
                consulta = consulta.Where(
                    o => o.Estado == estado
                );
            }

            int totalRegistros = await consulta.CountAsync();

            int totalPaginas = Math.Max(
                1,
                (int)Math.Ceiling(
                    totalRegistros / (double)RegistrosPorPagina
                )
            );

            if (pagina > totalPaginas)
                pagina = totalPaginas;

            var ordenes = await consulta
                .OrderByDescending(o => o.FechaOrden)
                .ThenByDescending(o => o.IdOrden)
                .Skip((pagina - 1) * RegistrosPorPagina)
                .Take(RegistrosPorPagina)
                .ToListAsync();

            ViewBag.PaginaActual = pagina;
            ViewBag.TotalPaginas = totalPaginas;
            ViewBag.TotalRegistros = totalRegistros;
            ViewBag.RegistrosPorPagina = RegistrosPorPagina;

            // Mantener los valores de los filtros en la vista.
            ViewBag.DireccionBuscada = direccion ?? "";
            ViewBag.EstadoBuscado = estado ?? "";

            return View(ordenes);
        }

        // GET: Orden/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var consulta = _context.Ordenes
                .Include(o => o.Usuario)
                .Include(o => o.Direccion)
                .Include(o => o.Detalles)
                    .ThenInclude(d => d.Producto)
                .AsQueryable();

            // El cliente solo puede consultar sus propias órdenes.
            if (User.IsInRole("Cliente"))
            {
                var valorId = User.FindFirstValue(
                    ClaimTypes.NameIdentifier
                );

                if (!int.TryParse(valorId, out int idUsuario))
                    return Forbid();

                consulta = consulta.Where(
                    o => o.IdUsuario == idUsuario
                );
            }

            var orden = await consulta.FirstOrDefaultAsync(
                o => o.IdOrden == id
            );

            if (orden == null)
                return NotFound();

            return View(orden);
        }

        // POST: Orden/CambiarEstado
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> CambiarEstado(
            int id,
            string estado
        )
        {
            var orden = await _context.Ordenes
                .FirstOrDefaultAsync(o => o.IdOrden == id);

            if (orden == null)
                return NotFound();

            var estadosPermitidos = new[]
            {
                "Pendiente",
                "Observado",
                "En camino",
                "Finalizado",
                "Cancelado"
            };

            if (string.IsNullOrWhiteSpace(estado) ||
                !estadosPermitidos.Contains(estado))
            {
                TempData["Error"] =
                    "El estado seleccionado no es válido.";

                return RedirectToAction(
                    nameof(Details),
                    new { id }
                );
            }

            orden.Estado = estado;

            await _context.SaveChangesAsync();

            TempData["Mensaje"] =
                $"La orden #{orden.IdOrden} se actualizó a '{estado}'.";

            return RedirectToAction(
                nameof(Details),
                new { id }
            );
        }

        // POST: Orden/Eliminar
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Eliminar(int id)
        {
            await using var transaccion =
                await _context.Database.BeginTransactionAsync();

            var orden = await _context.Ordenes
                .Include(o => o.Detalles)
                .FirstOrDefaultAsync(o => o.IdOrden == id);

            if (orden == null)
            {
                TempData["Error"] =
                    "La orden que intentás eliminar no existe.";

                return RedirectToAction(nameof(Index));
            }

            // No eliminar órdenes ya enviadas o finalizadas.
            if (orden.Estado == "En camino" ||
                orden.Estado == "Finalizado" ||
                orden.Estado == "Enviado" ||
                orden.Estado == "Entregado")
            {
                TempData["Error"] =
                    "No se puede eliminar una orden que ya fue enviada o finalizada.";

                return RedirectToAction(
                    nameof(Details),
                    new { id }
                );
            }

            var pagos = await _context.Pagos
                .Where(p => p.IdOrden == id)
                .ToListAsync();

            _context.Pagos.RemoveRange(pagos);
            _context.DetallesOrden.RemoveRange(orden.Detalles);
            _context.Ordenes.Remove(orden);

            await _context.SaveChangesAsync();
            await transaccion.CommitAsync();

            TempData["Mensaje"] =
                $"La orden #{id} se eliminó correctamente.";

            return RedirectToAction(nameof(Index));
        }



        [HttpGet]
        public async Task<IActionResult> SugerenciasDireccion(string? termino)
        {
            if (string.IsNullOrWhiteSpace(termino))
                return Json(new List<object>());

            termino = termino.Trim();

            var consulta = _context.Ordenes
                .AsNoTracking()
                .AsQueryable();

            // El cliente solo puede buscar entre sus propias órdenes.
            if (User.IsInRole("Cliente"))
            {
                var valorId = User.FindFirstValue(
                    ClaimTypes.NameIdentifier
                );

                if (!int.TryParse(valorId, out int idUsuario))
                    return Forbid();

                consulta = consulta.Where(o => o.IdUsuario == idUsuario);
            }

            var sugerencias = await consulta
                .Where(o =>
                    o.Direccion != null &&
                    (
                        (o.Direccion.Calle != null &&
                         o.Direccion.Calle.Contains(termino))
                        ||
                        (o.Direccion.Ciudad != null &&
                         o.Direccion.Ciudad.Contains(termino))
                    ))
                .Select(o => new
                {
                    calle = o.Direccion!.Calle ?? "",
                    ciudad = o.Direccion!.Ciudad ?? "",
                    numero = o.Direccion!.Numero
                })
                .Distinct()
                .Take(8)
                .ToListAsync();

            var resultado = sugerencias.Select(d =>
            {
                // Mostrar la calle con el número, si está disponible.
                string texto = string.Join(" ", new[]
                {
            d.calle,
            Convert.ToString(d.numero)
        }.Where(s => !string.IsNullOrWhiteSpace(s)));

                // Enviar un valor que coincida con el filtro del Index.
                string busqueda =
                    !string.IsNullOrWhiteSpace(d.calle) &&
                    d.calle.Contains(
                        termino,
                        StringComparison.OrdinalIgnoreCase
                    )
                        ? d.calle
                        : d.ciudad;

                return new
                {
                    texto,
                    ciudad = d.ciudad,
                    busqueda
                };
            }).ToList();

            return Json(resultado);
        }


    }
}
