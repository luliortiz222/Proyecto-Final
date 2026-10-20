

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.IO;
using System.Linq;
using ProyectoIntegradorFinal.Models;
using ProyectoIntegradorFinal.Data;

namespace ProyectoIntegradorFinal.Controllers
{
    public class UsuarioController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public UsuarioController(
            AppDbContext context,
            IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
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
        public async Task<IActionResult> Registro(
            Usuario usuario,
            IFormFile? avatarArchivo)
        {
            // Datos administrados por el servidor.
            usuario.Rol = "Cliente";
            usuario.FechaIngreso = DateTime.Now;

            // Se valida el archivo manualmente.
            ModelState.Remove(nameof(Usuario.Rol));
            ModelState.Remove(nameof(Usuario.avatar));
            ModelState.Remove(nameof(Usuario.Direccion));

            if (avatarArchivo == null || avatarArchivo.Length == 0)
            {
                ModelState.AddModelError(
                    "avatarArchivo",
                    "Debés seleccionar una imagen de perfil."
                );
            }
            else
            {
                string extension = Path
                    .GetExtension(avatarArchivo.FileName)
                    .ToLowerInvariant();

                string[] extensionesPermitidas =
                    { ".jpg", ".jpeg", ".png", ".webp" };

                if (!extensionesPermitidas.Contains(extension))
                {
                    ModelState.AddModelError(
                        "avatarArchivo",
                        "La imagen debe ser JPG, JPEG, PNG o WEBP."
                    );
                }

                if (avatarArchivo.Length > 2 * 1024 * 1024)
                {
                    ModelState.AddModelError(
                        "avatarArchivo",
                        "La imagen no puede superar los 2 MB."
                    );
                }
            }

            if (!ModelState.IsValid)
            {
                return View(usuario);
            }

            bool emailExiste = await _context.Usuarios
                .AnyAsync(u => u.Email == usuario.Email);

            if (emailExiste)
            {
                ModelState.AddModelError(
                    nameof(Usuario.Email),
                    "Ya existe una cuenta con ese correo electrónico."
                );

                return View(usuario);
            }

            bool dniExiste = await _context.Usuarios
                .AnyAsync(u => u.Dni == usuario.Dni);

            if (dniExiste)
            {
                ModelState.AddModelError(
                    nameof(Usuario.Dni),
                    "Ya existe una cuenta con ese DNI."
                );

                return View(usuario);
            }

            string carpeta = Path.Combine(
                _environment.WebRootPath,
                "images",
                "avatares"
            );

            Directory.CreateDirectory(carpeta);

            string extensionArchivo = Path
                .GetExtension(avatarArchivo!.FileName)
                .ToLowerInvariant();

            string nombreArchivo =
                $"{Guid.NewGuid()}{extensionArchivo}";

            string rutaCompleta = Path.Combine(
                carpeta,
                nombreArchivo
            );

            try
            {
                using (var stream = new FileStream(
                    rutaCompleta,
                    FileMode.CreateNew))
                {
                    await avatarArchivo.CopyToAsync(stream);
                }

                // Ruta relativa que se guarda en MySQL.
                usuario.avatar =
                    $"/images/avatares/{nombreArchivo}";

                _context.Usuarios.Add(usuario);
                await _context.SaveChangesAsync();

                TempData["Mensaje"] =
                    "Cuenta creada correctamente. Ya podés iniciar sesión.";

                return RedirectToAction(nameof(Login));
            }
            catch (Exception ex)
            {
                if (System.IO.File.Exists(rutaCompleta))
                {
                    System.IO.File.Delete(rutaCompleta);
                }

                Console.WriteLine(
                    $"Error al crear la cuenta: {ex}"
                );

                ModelState.AddModelError(
                    string.Empty,
                    "No se pudo crear la cuenta. Intentá nuevamente."
                );

                return View(usuario);
            }
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
                ViewBag.Error =
                    "Ingresá tu correo y contraseña.";

                return View();
            }

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
        usuario.Rol),

    new Claim(
        "Avatar",
        usuario.avatar ?? "")
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
