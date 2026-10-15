using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using ProyectoIntegradorFinal.Data;
using ProyectoIntegradorFinal.Models;
using System.Security.Claims;

namespace ProyectoIntegradorFinal.Controllers
{
    [Authorize(Roles = "Cliente")]
    public class ItemCarritoController : Controller
    {
        private readonly AppDbContext _context;

        public ItemCarritoController(AppDbContext context)
        {
            _context = context;
        }

        // GET: ItemCarrito/Index
        public async Task<IActionResult> Index()
        {
            int idUsuario = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!
            );

            var carrito = await _context.Carritos
                .Include(c => c.Items)
                    .ThenInclude(i => i.Producto)
                .FirstOrDefaultAsync(c => c.IdUsuario == idUsuario);

            if (carrito == null)
            {
                ViewBag.Total = 0m;
                return View(new List<ItemCarrito>());
            }

            var items = carrito.Items.ToList();

            ViewBag.Total = items.Sum(
                i => i.cantidad * i.Producto.Precio
            );

            return View(items);
        }

        // POST: ItemCarrito/Agregar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Agregar(
            int idProducto,
            int cantidad = 1)
        {
            if (cantidad <= 0)
            {
                TempData["Error"] =
                    "La cantidad debe ser mayor que cero.";

                return RedirectToAction("Index", "Productos");
            }

            int idUsuario = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!
            );

            var producto = await _context.Productos
                .FindAsync(idProducto);

            if (producto == null)
                return NotFound();

            var carrito = await _context.Carritos
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.IdUsuario == idUsuario);

            if (carrito == null)
            {
                carrito = new Carrito
                {
                    IdUsuario = idUsuario,
                    FechaActualizacion = DateTime.Now,
                    Items = new List<ItemCarrito>()
                };

                _context.Carritos.Add(carrito);
                await _context.SaveChangesAsync();
            }

            var item = await _context.ItemsCarrito
                .FirstOrDefaultAsync(i =>
                    i.IdCarrito == carrito.IdCarrito &&
                    i.IdProducto == idProducto);

            if (item == null)
            {
                item = new ItemCarrito
                {
                    IdCarrito = carrito.IdCarrito,
                    IdProducto = idProducto,
                    cantidad = cantidad
                };

                _context.ItemsCarrito.Add(item);
            }
            else
            {
                item.cantidad += cantidad;
            }

            carrito.FechaActualizacion = DateTime.Now;

            await _context.SaveChangesAsync();

            TempData["Mensaje"] =
                "Producto agregado al carrito correctamente.";

            return RedirectToAction(nameof(Index));
        }

        // GET: ItemCarrito/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            int idUsuario = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!
            );

            var item = await _context.ItemsCarrito
                .Include(i => i.Carrito)
                .Include(i => i.Producto)
                .FirstOrDefaultAsync(i =>
                    i.IdItem == id &&
                    i.Carrito.IdUsuario == idUsuario);

            if (item == null)
                return NotFound();

            return View(item);
        }

        // POST: ItemCarrito/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            int cantidad)
        {
            if (cantidad <= 0)
            {
                ModelState.AddModelError(
                    "cantidad",
                    "La cantidad debe ser mayor que cero.");
            }

            int idUsuario = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!
            );

            var item = await _context.ItemsCarrito
                .Include(i => i.Carrito)
                .FirstOrDefaultAsync(i =>
                    i.IdItem == id &&
                    i.Carrito.IdUsuario == idUsuario);

            if (item == null)
                return NotFound();

            if (!ModelState.IsValid)
                return View(item);

            item.cantidad = cantidad;
            item.Carrito.FechaActualizacion = DateTime.Now;

            await _context.SaveChangesAsync();

            TempData["Mensaje"] =
                "Cantidad actualizada correctamente.";

            return RedirectToAction(nameof(Index));
        }

        // POST: ItemCarrito/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            int idUsuario = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!
            );

            var item = await _context.ItemsCarrito
                .Include(i => i.Carrito)
                .FirstOrDefaultAsync(i =>
                    i.IdItem == id &&
                    i.Carrito.IdUsuario == idUsuario);

            if (item == null)
                return NotFound();

            item.Carrito.FechaActualizacion = DateTime.Now;

            _context.ItemsCarrito.Remove(item);

            await _context.SaveChangesAsync();

            TempData["Mensaje"] =
                "Producto eliminado del carrito.";

            return RedirectToAction(nameof(Index));
        }
    }
}