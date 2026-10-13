using System.ComponentModel.DataAnnotations;
using System.Data;

namespace ProyectoIntegradorFinal.Models
{
    public class Usuario
    {
        [Key]
        public int IdUsuario { get; set; }
        [Required]
        public string Nombre { get; set; }
        [Required]
        public string Apellido { get; set; }
        [Required]
        public string Dni { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public string Rol { get; set; }
        [Required]
        public string avatar { get; set; }

        public DateTime FechaIngreso { get; set; }


        public virtual ICollection<Direccion> Direccion { get; set; }
    }
    
    
}
