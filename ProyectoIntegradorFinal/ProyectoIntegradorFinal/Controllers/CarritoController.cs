using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using ProyectoIntegradorFinal.Data;
namespace ProyectoIntegradorFinal.Controllers
{
    [Authorize(Roles = "Cliente")]
    public class CarritoController : Controller
    {
        private readonly AppDbContext _context;

        public CarritoController(AppDbContext context)
        {
            _context = context;
        }

        // Obtiene el ID del cliente que inició sesión.
        private bool IntentarObtenerUsuarioId(out int usuarioId)
        {
            return int.TryParse(
                User.FindFirstValue(ClaimTypes.NameIdentifier),
                out usuarioId
            );
        }

        // GET: Carrito
        public async Task<IActionResult> Index()
        {
            int usuarioId = 1; // despues reemplazamos con el ID del usuario actual

            if (usuarioId == 0)
            {
                // Si no está logueado, lo mandamos al Login
                return RedirectToAction("Login", "Usuarios");
            }

            var carrito = await _context.Carritos
                .Include(c => c.Items)
                    .ThenInclude(i => i.Producto)
                .FirstOrDefaultAsync(c => c.IdUsuario == usuarioId);

            if (carrito == null)
            {
                carrito = new Carrito
                {
                    IdUsuario = usuarioId,
                    FechaActualizacion = DateTime.Now
                };

                _context.Carritos.Add(carrito);
                await _context.SaveChangesAsync();
            }

            return View(carrito);
        }

        // POST: Carrito/Agregar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Agregar(int IdProducto, int cantidad)
        {
            int usuarioId = 1; // despues reemplazamos con el ID del usuario actual
            var carrito = await _context.Carritos
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.IdUsuario == usuarioId);

            if (carrito == null)
            {
                carrito = new Carrito
                {
                    IdUsuario = usuarioId,
                    FechaActualizacion = DateTime.Now
                };

                _context.Carritos.Add(carrito);
                await _context.SaveChangesAsync();
            }

            var itemExistente = await _context.ItemsCarrito
                .FirstOrDefaultAsync(i =>
                    i.IdCarrito == carrito.IdCarrito &&
                    i.IdProducto == IdProducto);

            int cantidadActual = itemExistente?.cantidad ?? 0;

            if (cantidadActual + cantidad > producto.Stock)
            {
                TempData["Error"] =
                    $"Solo hay {producto.Stock} unidades disponibles de este perfume.";

                return RedirectToAction(nameof(Index));
            }

            if (itemExistente != null)
            {
                itemExistente.cantidad += cantidad;
            }
            else
            {
                var nuevoItem = new ItemCarrito
                {
                    IdProducto = IdProducto,
                    cantidad = cantidad,
                    IdCarrito = carrito.IdCarrito
                };

                _context.ItemsCarrito.Add(nuevoItem);
            }

            carrito.FechaActualizacion = DateTime.Now;

            await _context.SaveChangesAsync();

            TempData["Mensaje"] =
                "¡Perfume agregado al carrito correctamente!";

            return RedirectToAction(nameof(Index));
        }

        // POST: Carrito/EliminarItem
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarItem(int itemId)
        {
            if (!IntentarObtenerUsuarioId(out int usuarioId))
            {
                return Challenge();
            }

            var item = await _context.ItemsCarrito
                .Include(i => i.Carrito)
                .FirstOrDefaultAsync(i =>
                    i.IdItem == itemId &&
                    i.Carrito.IdUsuario == usuarioId);

            if (item == null)
            {
                return NotFound();
            }

            item.Carrito.FechaActualizacion = DateTime.Now;

            _context.ItemsCarrito.Remove(item);

            await _context.SaveChangesAsync();

            TempData["Mensaje"] =
                "Producto eliminado del carrito.";

            return RedirectToAction(nameof(Index));
        }

        // POST: Carrito/ActualizarCantidad
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActualizarCantidad(int itemId, int cantidad)
        {
            int usuarioId = 1; // Temporal.

            if (usuarioId == 0)
            {
                // Si no está logueado, lo mandamos al Login
                return RedirectToAction("Login", "Usuarios");
            }

            var item = await _context.ItemsCarrito
                .Include(i => i.Carrito)
                .Include(i => i.Producto)
                .FirstOrDefaultAsync(i =>
                    i.IdItem == itemId &&
                    i.Carrito.IdUsuario == usuarioId);

            if (item == null)
            {
                return NotFound();
            }

            if (item.Producto.Stock < 1)
            {
                TempData["Error"] =
                    "Este perfume no tiene stock disponible.";

                return RedirectToAction(nameof(Index));
            }

            if (cantidad < 1 || cantidad > item.Producto.Stock)
            {
                TempData["Error"] =
                    $"La cantidad debe estar entre 1 y {item.Producto.Stock}.";

                return RedirectToAction(nameof(Index));
            }

            item.cantidad = cantidad;
            item.Carrito.FechaActualizacion = DateTime.Now;

            await _context.SaveChangesAsync();

            TempData["Mensaje"] =
                "Cantidad actualizada correctamente.";

            return RedirectToAction(nameof(Index));
        }

    }
}