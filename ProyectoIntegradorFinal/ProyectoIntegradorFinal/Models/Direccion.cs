using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoIntegradorFinal.Models
{
    public class Direccion
    {
        [Key]
        public int IdDireccion { get; set; }
        [Required]
        public string Calle { get; set; }
        [Required]
        public string Numero { get; set; }
        [Required]
        public string Ciudad { get; set; }
        [Required]
        public string Provincia { get; set; }
        [Required]
        public string CodigoPostal { get; set; }
        // Foreign key to Usuario
        public int IdUsuario { get; set; }
        [ForeignKey("IdUsuario")]
        public virtual Usuario Usuario { get; set; }
    
    }
}
