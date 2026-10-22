
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace ProyectoIntegradorFinal.ViewModels
{
    public class PerfilUsuarioViewModel
    {
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El apellido es obligatorio.")]
        public string Apellido { get; set; } = string.Empty;

        [Required(ErrorMessage = "El DNI es obligatorio.")]
        public string Dni { get; set; } = string.Empty;

        [Required(ErrorMessage = "El email es obligatorio.")]
        [EmailAddress(ErrorMessage = "Ingresá un email válido.")]
        public string Email { get; set; } = string.Empty;

        public string? AvatarActual { get; set; }

        public IFormFile? AvatarArchivo { get; set; }

        [DataType(DataType.Password)]
        public string? PasswordActual { get; set; }

        [DataType(DataType.Password)]
        public string? NuevaPassword { get; set; }

        [DataType(DataType.Password)]
        [Compare(
            nameof(NuevaPassword),
            ErrorMessage = "Las contraseñas nuevas no coinciden.")]
        public string? ConfirmarPassword { get; set; }
    }
}
