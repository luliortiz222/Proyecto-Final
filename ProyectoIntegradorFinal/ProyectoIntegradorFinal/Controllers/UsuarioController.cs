
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.IO;
using System.Linq;
using ProyectoIntegradorFinal.Models;
using ProyectoIntegradorFinal.Data;
using ProyectoIntegradorFinal.ViewModels;

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
            // Los datos sensibles del sistema los administra el servidor.
            usuario.Rol = "Cliente";
            usuario.FechaIngreso = DateTime.Now;

            usuario.Nombre = usuario.Nombre?.Trim() ?? "";
            usuario.Apellido = usuario.Apellido?.Trim() ?? "";
            usuario.Dni = usuario.Dni?.Trim() ?? "";
            usuario.Email = usuario.Email?.Trim() ?? "";

            ModelState.Remove(nameof(Usuario.Rol));
            ModelState.Remove(nameof(Usuario.avatar));
            ModelState.Remove(nameof(Usuario.Direccion));

            if (avatarArchivo == null || avatarArchivo.Length == 0)
            {
                ModelState.AddModelError(
                    "avatarArchivo",
                    "Debés seleccionar una imagen de perfil.");
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
                        "La imagen debe ser JPG, JPEG, PNG o WEBP.");
                }

                if (avatarArchivo.Length > 2 * 1024 * 1024)
                {
                    ModelState.AddModelError(
                        "avatarArchivo",
                        "La imagen no puede superar los 2 MB.");
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
                    "Ya existe una cuenta con ese correo electrónico.");

                return View(usuario);
            }

            bool dniExiste = await _context.Usuarios
                .AnyAsync(u => u.Dni == usuario.Dni);

            if (dniExiste)
            {
                ModelState.AddModelError(
                    nameof(Usuario.Dni),
                    "Ya existe una cuenta con ese DNI.");

                return View(usuario);
            }

            string carpeta = Path.Combine(
                _environment.WebRootPath,
                "images",
                "avatares");

            Directory.CreateDirectory(carpeta);

            string extensionArchivo = Path
                .GetExtension(avatarArchivo!.FileName)
                .ToLowerInvariant();

            string nombreArchivo =
                $"{Guid.NewGuid()}{extensionArchivo}";

            string rutaCompleta = Path.Combine(
                carpeta,
                nombreArchivo);

            try
            {
                using (var stream = new FileStream(
                    rutaCompleta,
                    FileMode.CreateNew))
                {
                    await avatarArchivo.CopyToAsync(stream);
                }

                usuario.avatar =
                    $"/images/avatares/{nombreArchivo}";

                // Hash de contraseña con BCrypt.
                usuario.Password =
                    BCrypt.Net.BCrypt.HashPassword(usuario.Password);

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
                    $"Error al crear la cuenta: {ex}");

                ModelState.AddModelError(
                    string.Empty,
                    "No se pudo crear la cuenta. Intentá nuevamente.");

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

            email = email.Trim();

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Email == email);

            bool passwordValida = false;

            if (usuario != null &&
                !string.IsNullOrWhiteSpace(usuario.Password))
            {
                try
                {
                    passwordValida =
                        BCrypt.Net.BCrypt.Verify(
                            password,
                            usuario.Password);
                }
                catch (BCrypt.Net.SaltParseException)
                {
                    // El hash almacenado no tiene formato BCrypt válido.
                    passwordValida = false;
                }
            }

            if (usuario == null || !passwordValida)
            {
                ViewBag.Error =
                    "Correo o contraseña incorrectos.";

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

            return RedirectToAction(nameof(Login));
        }

        // GET: /Usuario/Perfil
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Perfil()
        {
            var idClaim =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(idClaim, out int idUsuario))
            {
                return Challenge();
            }

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(
                    u => u.IdUsuario == idUsuario);

            if (usuario == null)
            {
                return NotFound();
            }

            var modelo = new PerfilUsuarioViewModel
            {
                Nombre = usuario.Nombre,
                Apellido = usuario.Apellido,
                Dni = usuario.Dni,
                Email = usuario.Email,
                AvatarActual = usuario.avatar
            };

            return View(modelo);
        }

        // POST: /Usuario/Perfil
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Perfil(
            PerfilUsuarioViewModel modelo)
        {
            var idClaim =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(idClaim, out int idUsuario))
            {
                return Challenge();
            }

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(
                    u => u.IdUsuario == idUsuario);

            if (usuario == null)
            {
                return NotFound();
            }

            modelo.AvatarActual = usuario.avatar;

            // La contraseña es opcional al editar el perfil.
            bool cambiarPassword =
                !string.IsNullOrWhiteSpace(modelo.PasswordActual) ||
                !string.IsNullOrWhiteSpace(modelo.NuevaPassword) ||
                !string.IsNullOrWhiteSpace(modelo.ConfirmarPassword);

            if (cambiarPassword)
            {
                if (string.IsNullOrWhiteSpace(modelo.PasswordActual) ||
                    string.IsNullOrWhiteSpace(modelo.NuevaPassword) ||
                    string.IsNullOrWhiteSpace(modelo.ConfirmarPassword))
                {
                    ModelState.AddModelError(
                        string.Empty,
                        "Para cambiar la contraseña, completá los tres campos.");
                }
                else
                {
                    bool passwordActualValida = false;

                    try
                    {
                        passwordActualValida =
                            BCrypt.Net.BCrypt.Verify(
                                modelo.PasswordActual,
                                usuario.Password);
                    }
                    catch (BCrypt.Net.SaltParseException)
                    {
                        passwordActualValida = false;
                    }

                    if (!passwordActualValida)
                    {
                        ModelState.AddModelError(
                            nameof(modelo.PasswordActual),
                            "La contraseña actual es incorrecta.");
                    }

                    if (modelo.NuevaPassword.Length < 8)
                    {
                        ModelState.AddModelError(
                            nameof(modelo.NuevaPassword),
                            "La nueva contraseña debe tener al menos 8 caracteres.");
                    }

                    if (modelo.NuevaPassword != modelo.ConfirmarPassword)
                    {
                        ModelState.AddModelError(
                            nameof(modelo.ConfirmarPassword),
                            "Las contraseñas nuevas no coinciden.");
                    }
                }
            }

            // Validación de correo duplicado.
            if (!string.IsNullOrWhiteSpace(modelo.Email))
            {
                modelo.Email = modelo.Email.Trim();

                bool emailExiste = await _context.Usuarios
                    .AnyAsync(u =>
                        u.Email == modelo.Email &&
                        u.IdUsuario != idUsuario);

                if (emailExiste)
                {
                    ModelState.AddModelError(
                        nameof(modelo.Email),
                        "Ese correo ya está registrado.");
                }
            }

            // Validación de DNI duplicado.
            if (!string.IsNullOrWhiteSpace(modelo.Dni))
            {
                modelo.Dni = modelo.Dni.Trim();

                bool dniExiste = await _context.Usuarios
                    .AnyAsync(u =>
                        u.Dni == modelo.Dni &&
                        u.IdUsuario != idUsuario);

                if (dniExiste)
                {
                    ModelState.AddModelError(
                        nameof(modelo.Dni),
                        "Ese DNI ya está registrado.");
                }
            }

            // Validación del avatar opcional.
            if (modelo.AvatarArchivo != null &&
                modelo.AvatarArchivo.Length > 0)
            {
                string extension = Path
                    .GetExtension(modelo.AvatarArchivo.FileName)
                    .ToLowerInvariant();

                string[] extensionesPermitidas =
                    { ".jpg", ".jpeg", ".png", ".webp" };

                if (!extensionesPermitidas.Contains(extension))
                {
                    ModelState.AddModelError(
                        nameof(modelo.AvatarArchivo),
                        "La imagen debe ser JPG, JPEG, PNG o WEBP.");
                }

                if (modelo.AvatarArchivo.Length > 2 * 1024 * 1024)
                {
                    ModelState.AddModelError(
                        nameof(modelo.AvatarArchivo),
                        "La imagen no puede superar los 2 MB.");
                }
            }

            if (!ModelState.IsValid)
            {
                return View(modelo);
            }

            usuario.Nombre = modelo.Nombre.Trim();
            usuario.Apellido = modelo.Apellido.Trim();
            usuario.Dni = modelo.Dni.Trim();
            usuario.Email = modelo.Email.Trim();

            // Guardar la nueva contraseña también con BCrypt.
            if (cambiarPassword)
            {
                usuario.Password =
                    BCrypt.Net.BCrypt.HashPassword(
                        modelo.NuevaPassword!);
            }

            if (modelo.AvatarArchivo != null &&
                modelo.AvatarArchivo.Length > 0)
            {
                string carpeta = Path.Combine(
                    _environment.WebRootPath,
                    "images",
                    "avatares");

                Directory.CreateDirectory(carpeta);

                string nombreArchivo =
                    $"{Guid.NewGuid()}{Path.GetExtension(modelo.AvatarArchivo.FileName).ToLowerInvariant()}";

                string rutaCompleta = Path.Combine(
                    carpeta,
                    nombreArchivo);

                using (var stream = new FileStream(
                    rutaCompleta,
                    FileMode.CreateNew))
                {
                    await modelo.AvatarArchivo.CopyToAsync(stream);
                }

                usuario.avatar =
                    $"/images/avatares/{nombreArchivo}";
            }

            await _context.SaveChangesAsync();

            // Actualizar las cookies con los datos nuevos.
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

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identidad));

            TempData["MensajePerfil"] =
                "Tu perfil se actualizó correctamente.";

            return RedirectToAction(nameof(Perfil));
        }
    }
}
