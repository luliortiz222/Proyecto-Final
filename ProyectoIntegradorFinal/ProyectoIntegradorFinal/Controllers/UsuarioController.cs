
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using ProyectoIntegradorFinal.Models;
using ProyectoIntegradorFinal.Data;

namespace ProyectoIntegradorFinal.Controllers
{
    public class UsuarioController : Controller
    {
        private readonly AppDbContext _context;

        public UsuarioController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Usuario/Registro
        [HttpGet]
        public IActionResult Registro()
        {
            return View();
        }

        // POST: /Usuario/Registro
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registro(Usuario usuario)
        {
            if (!ModelState.IsValid)
                return View(usuario);

            bool emailExiste = await _context.Usuarios
                .AnyAsync(u => u.Email == usuario.Email);

            if (emailExiste)
            {
                ModelState.AddModelError(
                    "Email",
                    "Ya existe una cuenta con ese correo electrónico.");

                return View(usuario);
            }

            bool dniExiste = await _context.Usuarios
                .AnyAsync(u => u.Dni == usuario.Dni);

            if (dniExiste)
            {
                ModelState.AddModelError(
                    "DNI",
                    "Ya existe una cuenta con ese DNI.");

                return View(usuario);
            }

            // Por ahora guardamos la contraseña sin hash.
            usuario.Rol = "Cliente, Administrador";
            usuario.FechaIngreso = DateTime.Now;
            usuario.avatar = "default.png";

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            TempData["Mensaje"] =
                "Cuenta creada correctamente. Ya podés iniciar sesión.";

            return RedirectToAction("Login");
        }

        // GET: /Usuario/Login
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Usuario/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(
            string email,
            string password)
        {
            if (string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Ingresá tu correo y contraseña.";
                return View();
            }

            // Comparación directa, sin hash, por ahora.
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u =>
                    u.Email == email &&
                    u.Password == password);

            if (usuario == null)
            {
                ViewBag.Error =
                    "El correo electrónico o la contraseña son incorrectos.";

                return View();
            }

            var claims = new List<Claim>
            {
                new Claim(
                    ClaimTypes.NameIdentifier,
                    usuario.IdUsuario.ToString()),

                new Claim(
                    ClaimTypes.Name,
                    usuario.Nombre + " " + usuario.Apellido),

                new Claim(
                    ClaimTypes.Email,
                    usuario.Email),

                new Claim(
                    ClaimTypes.Role,
                    usuario.Rol)
            };

            var identidad = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme);

            var principal = new ClaimsPrincipal(identidad);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal);

            return RedirectToAction("Index", "Home");
        }

        // POST: /Usuario/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Login");
        }
    }
}
