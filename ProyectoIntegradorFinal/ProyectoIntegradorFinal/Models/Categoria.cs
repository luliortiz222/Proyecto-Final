using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ProyectoIntegradorFinal.Models
{
    public class Categoria
    {
        [Key]
        public int IdCategoria { get; set; }

        [Required]
        public string Nombre { get; set; }

        // Propiedad de navegación: Una categoría tiene muchos productos
        [JsonIgnore]
        public virtual ICollection<Producto>? Productos { get; set; }
    }
}
