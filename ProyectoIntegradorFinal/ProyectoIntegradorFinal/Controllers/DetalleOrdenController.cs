using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using ProyectoIntegradorFinal.Data;
using System.Security.Claims;

namespace ProyectoIntegradorFinal.Controllers
{
    [Authorize(Roles = "Cliente,Administrador")]
    public class DetalleOrdenController : Controller
    {
        private readonly AppDbContext _context;

        public DetalleOrdenController(AppDbContext context)
        {
            _context = context;
        }

        // GET: DetalleOrden/Index/5
        // Recibe el IdOrden.
        public async Task<IActionResult> Index(int? id)
        {
            if (id == null)
                return NotFound();

            var consulta = _context.DetallesOrden
                .Include(d => d.Producto)
                .Include(d => d.Orden)
                .AsQueryable();

            // El cliente solo puede ver los detalles de sus propias órdenes.
            if (User.IsInRole("Cliente"))
            {
                int idUsuario = int.Parse(
                    User.FindFirstValue(ClaimTypes.NameIdentifier)!
                );

                consulta = consulta.Where(
                    d => d.Orden.IdUsuario == idUsuario
                );
            }

            var detalles = await consulta
                .Where(d => d.IdOrden == id.Value)
                .ToListAsync();

            // Verificamos que la orden exista y que el usuario tenga acceso.
            bool ordenPermitida = await _context.Ordenes
                .AnyAsync(o =>
                    o.IdOrden == id.Value &&
                    (User.IsInRole("Administrador") ||
                     o.IdUsuario == int.Parse(
                         User.FindFirstValue(ClaimTypes.NameIdentifier)!
                     ))
                );

            if (!ordenPermitida)
                return NotFound();

            return View(detalles);
        }

        // GET: DetalleOrden/Details/5
        // Recibe el IdDetalle.
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var consulta = _context.DetallesOrden
                .Include(d => d.Producto)
                .Include(d => d.Orden)
                .AsQueryable();

            if (User.IsInRole("Cliente"))
            {
                int idUsuario = int.Parse(
                    User.FindFirstValue(ClaimTypes.NameIdentifier)!
                );

                consulta = consulta.Where(
                    d => d.Orden.IdUsuario == idUsuario
                );
            }

            var detalle = await consulta
                .FirstOrDefaultAsync(d => d.IdDetalle == id);

            if (detalle == null)
                return NotFound();

            return View(detalle);
        }
    }
}