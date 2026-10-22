using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using ProyectoIntegradorFinal.Models;
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

        private int ObtenerUsuarioId()
        {
            // Buscamos el identificador del usuario en los claims de la sesión actual
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (int.TryParse(userIdClaim, out int usuarioId))
            {
                return usuarioId;
            }

            return 0; 
        }

        // GET: Carrito
        public async Task<IActionResult> Index()
        {
            int usuarioId = ObtenerUsuarioId();

            if (usuarioId == 0)
            {
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

            ViewBag.Direcciones = await _context.Direccion
                .Where(d => d.IdUsuario == usuarioId)
                .OrderBy(d => d.Ciudad)
                .ThenBy(d => d.Calle)
                .ToListAsync();

            return View(carrito);
        }

        // POST: Carrito/Agregar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Agregar(int IdProducto, int cantidad)
        {
            var usuarioId = ObtenerUsuarioId();

            if (usuarioId == 0)
            {
                return Challenge();
            }

            if (cantidad < 1)
            {
                TempData["Error"] = "La cantidad debe ser mayor que cero.";
                return RedirectToAction(nameof(Index));
            }

            // Buscar el producto en la base de datos.
            var producto = await _context.Productos
                .FirstOrDefaultAsync(p => p.IdProducto == IdProducto);

            if (producto == null)
            {
                return NotFound();
            }

            if (producto.Stock < 1)
            {
                TempData["Error"] = "Este perfume no tiene stock disponible.";
                return RedirectToAction(nameof(Index));
            }

            // Buscar el carrito del usuario autenticado.
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

            TempData["Mensaje"] = "¡Perfume agregado al carrito correctamente!";

            return RedirectToAction(nameof(Index));
        }


        // POST: Carrito/EliminarItem
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarItem(int itemId)
        {
            var usuarioId = ObtenerUsuarioId();

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
            int usuarioId = ObtenerUsuarioId();

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



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FinalizarCompra(int idDireccion)
        {
            int usuarioActual = ObtenerUsuarioId();

            if (usuarioActual == 0)
                return Challenge();

            var carrito = await _context.Carritos
                .Include(c => c.Items)
                    .ThenInclude(i => i.Producto)
                .FirstOrDefaultAsync(c => c.IdUsuario == usuarioActual);

            if (carrito == null || carrito.Items == null || !carrito.Items.Any())
            {
                TempData["Error"] = "Tu carrito está vacío.";
                return RedirectToAction(nameof(Index));
            }

            // Verificar que la dirección elegida pertenezca al cliente.
            var direccion = await _context.Direccion
                .FirstOrDefaultAsync(d =>
                    d.IdDireccion == idDireccion &&
                    d.IdUsuario == usuarioActual);

            if (direccion == null)
            {
                TempData["Error"] = "Seleccioná una dirección de entrega válida.";
                return RedirectToAction(nameof(Index));
            }

            foreach (var item in carrito.Items)
            {
                if (item.Producto == null ||
                    item.cantidad < 1 ||
                    item.Producto.Stock < item.cantidad)
                {
                    TempData["Error"] =
                        $"No hay stock suficiente para {item.Producto?.Nombre ?? "uno de los productos"}. Revisá tu carrito.";

                    return RedirectToAction(nameof(Index));
                }
            }

            decimal totalOrden = carrito.Items.Sum(
                item => item.cantidad * item.Producto!.Precio
            );

            var orden = new Orden
            {
                IdUsuario = usuarioActual,
                IdDireccion = direccion.IdDireccion,
                FechaOrden = DateTime.Now,
                Total = totalOrden,
                Estado = "Pendiente"
            };

            foreach (var item in carrito.Items)
            {
                var detalleOrden = new DetalleOrden
                {
                    IdProducto = item.IdProducto,
                    Cantidad = item.cantidad,
                    PrecioUnitario = item.Producto!.Precio
                };

                orden.Detalles.Add(detalleOrden);
                item.Producto.Stock -= item.cantidad;
            }

            _context.Ordenes.Add(orden);
            await _context.SaveChangesAsync();

            var pago = new Pago
            {
                IdOrden = orden.IdOrden,
                FechaPago = DateTime.Now,
                Monto = totalOrden,
                MetodoPago = "Pendiente de selección",
                EstadoPago = "Pendiente"
            };

            _context.Pagos.Add(pago);
            _context.ItemsCarrito.RemoveRange(carrito.Items);
            carrito.FechaActualizacion = DateTime.Now;

            await _context.SaveChangesAsync();

            TempData["Mensaje"] =
                "Tu orden se creó correctamente. El pago está pendiente.";

            return RedirectToAction("Details", "Orden", new { id = orden.IdOrden });
        }


    }
}