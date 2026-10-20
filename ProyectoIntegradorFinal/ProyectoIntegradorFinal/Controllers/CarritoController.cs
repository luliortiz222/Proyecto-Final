using Microsoft.AspNetCore.Mvc;
using ProyectoIntegradorFinal.Models;
using Microsoft.EntityFrameworkCore;
using ProyectoIntegradorFinal.Data;
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
            int usuarioId = 1; // despues reemplazamos con el ID del usuario actual

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
            int usuarioId = 1; // despues reemplazamos con el ID del usuario actual
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
    }
}