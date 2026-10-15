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

        public OrdenController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Orden/Index
        public async Task<IActionResult> Index()
        {
            var consulta = _context.Ordenes
                .Include(o => o.Usuario)
                .Include(o => o.Direccion)
                .AsQueryable();

            // El cliente solo ve sus propias órdenes.
            if (User.IsInRole("Cliente"))
            {
                int idUsuario = int.Parse(
                    User.FindFirstValue(ClaimTypes.NameIdentifier)!
                );

                consulta = consulta.Where(o => o.IdUsuario == idUsuario);
            }

            var ordenes = await consulta
                .OrderByDescending(o => o.FechaOrden)
                .ToListAsync();

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
                .AsQueryable();

            // El cliente solo puede consultar sus propias órdenes.
            if (User.IsInRole("Cliente"))
            {
                int idUsuario = int.Parse(
                    User.FindFirstValue(ClaimTypes.NameIdentifier)!
                );

                consulta = consulta.Where(o => o.IdUsuario == idUsuario);
            }

            var orden = await consulta
                .FirstOrDefaultAsync(o => o.IdOrden == id);

            if (orden == null)
                return NotFound();

            return View(orden);
        }
    }
}
