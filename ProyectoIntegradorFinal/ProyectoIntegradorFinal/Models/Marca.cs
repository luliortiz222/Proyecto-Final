using System.ComponentModel.DataAnnotations;

namespace ProyectoIntegradorFinal.Models
{
    public class Marca
    {
        [Key]
        public int IdMarca { get; set; }

        [Required]
        public string Nombre { get; set;}

        public virtual ICollection<Producto>? Productos { get; set; }
    }
}
