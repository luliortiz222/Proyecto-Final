using Microsoft.AspNetCore.Mvc;
using ProyectoIntegradorFinal.Models;
using Microsoft.EntityFrameworkCore;
using ProyectoIntegradorFinal.Data;
using System.Security.Claims;
namespace ProyectoIntegradorFinal.Controllers
{
    public class CarritoController : Controller
    {
        private readonly AppDbContext _context;
        public CarritoController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Carrito
        public async Task<IActionResult> Index()
        {
            int usuarioId = ObtenerUsuarioId();

            if (usuarioId == 0)
            {
                // Si no está logueado, lo mandamos al Login
                return RedirectToAction("Login", "Usuarios");
            }

            var carrito = await _context.Carritos
                .Include(c => c.Items).ThenInclude(i => i.Producto).FirstOrDefaultAsync(c => c.IdUsuario == usuarioId);


            if (carrito == null)
            {
                // Si no hay un carrito para el usuario, crea uno nuevo
                carrito = new Carrito { IdUsuario = usuarioId, FechaActualizacion = DateTime.Now };
                _context.Carritos.Add(carrito);
                await _context.SaveChangesAsync();
            }


            return View(carrito);
        }


        // POST: Carrito/AgregarItem
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Agregar(int IdProducto, int cantidad)
        {
            int usuarioId = ObtenerUsuarioId();

            if (usuarioId == 0)
            {
                // Si no está logueado, lo mandamos al Login
                return RedirectToAction("Login", "Usuarios");
            }

            var carrito = await _context.Carritos
                .Include(c => c.Items).FirstOrDefaultAsync(c => c.IdUsuario == usuarioId);

            if (carrito == null)
            {
                // Si no hay un carrito para el usuario, crea uno nuevo
                carrito = new Carrito { IdUsuario = usuarioId, FechaActualizacion = DateTime.Now };
                _context.Carritos.Add(carrito);
                await _context.SaveChangesAsync();
            }


            var itemExistente = carrito.Items?.FirstOrDefault(i => i.IdProducto == IdProducto);

            if (itemExistente != null)
            {
                // Si el item ya existe, actualiza la cantidad
                itemExistente.cantidad += cantidad;
            }
            else
            {
                // Si el item no existe, crea uno nuevo
                var nuevoItem = new ItemCarrito { IdProducto = IdProducto, cantidad = cantidad, IdCarrito = carrito.IdCarrito };
                _context.ItemsCarrito.Add(nuevoItem);
            }

            carrito.FechaActualizacion = DateTime.Now;
            await _context.SaveChangesAsync();

            string referer = Request.Headers["Referer"].ToString();

            if (!string.IsNullOrEmpty(referer))
            {
                // Si existe la URL anterior, lo devolvemos ahí
                return Redirect(referer);
            }

            // Por si acaso no hay referer, lo mandamos al Home por defecto
            return RedirectToAction("Index", "Home");
        }

        // POST: Carrito/EliminarItem (Para sacar un perfume de la bolsa)
        [HttpPost]
        public async Task<IActionResult> EliminarItem(int itemId)
        {
            var item = await _context.ItemsCarrito.FindAsync(itemId);
            if (item != null)
            {
                _context.ItemsCarrito.Remove(item);
                await _context.SaveChangesAsync();
            }
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
                TempData["Error"] = "Este perfume no tiene stock disponible.";
                return RedirectToAction(nameof(Index));
            }

            if (cantidad < 1 || cantidad > item.Producto.Stock)
            {
                TempData["Error"] = "La cantidad debe estar entre 1 y el stock disponible.";
                return RedirectToAction(nameof(Index));
            }

            item.cantidad = cantidad;
            item.Carrito.FechaActualizacion = DateTime.Now;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private int ObtenerUsuarioId()
        {
            
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (int.TryParse(userIdClaim, out int usuarioId))
            {
                return usuarioId;
            }

            return 0; // Retorna 0 si no hay usuario logueado
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FinalizarCompra()
        {
            int usuarioIdActual = ObtenerUsuarioId();

            if (usuarioIdActual == 0)
            {
                // Si no está logueado, lo mandamos al Login
                return RedirectToAction("Login", "Usuarios");
            }

            var carrito = await _context.Carritos
                .Include(c => c.Items).ThenInclude(i => i.Producto)
                .FirstOrDefaultAsync(c => c.IdUsuario == usuarioIdActual);
            if (carrito == null || carrito.Items == null || !carrito.Items.Any())
            {
                TempData["Error"] = "No hay items en el carrito para finalizar la compra.";
                return RedirectToAction(nameof(Index));
            }

            var direccionEnvio = await _context.Direccion.FirstOrDefaultAsync(d => d.IdUsuario == usuarioIdActual);

            if (direccionEnvio == null)
            {
                TempData["Error"] = "No hay una dirección de envío asociada al usuario.";
                return RedirectToAction("Index","Direcciones");
            }


            decimal totalOrden = 0;
            foreach (var item in carrito.Items) 
            { 
                if (item.Producto.Stock < item.cantidad)
                {
                    TempData["Error"] = $"Lo sentimos, no hay suficiente stock para el perfume: {item.Producto.Nombre}. Stock disponible: {item.Producto.Stock}";
                    return RedirectToAction(nameof(Index));
                }
                totalOrden += item.Producto.Precio * item.cantidad;
            }

            var orden = new Orden
            {
                IdUsuario = usuarioIdActual,
                FechaOrden = DateTime.Now,
                Total = totalOrden,
                IdDireccion = direccionEnvio.IdDireccion,
                Estado = "Pendiente"
                
            };

            foreach (var item in carrito.Items)
            {
                var detalle = new DetalleOrden
                {
                    IdProducto = item.IdProducto,
                    Cantidad = item.cantidad,
                    PrecioUnitario = item.Producto.Precio
                };

                item.Producto.Stock -= item.cantidad;
                _context.Productos.Update(item.Producto);
            }

            _context.Ordenes.Add(orden);

            _context.ItemsCarrito.RemoveRange(carrito.Items);

            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Ordenes");



        }

    }
}